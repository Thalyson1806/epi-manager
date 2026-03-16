#!/usr/bin/env python3
"""
biometric_bridge.py — Bridge biométrico Linux usando libfprint-2
Todas as operações GLib/libfprint rodam numa única thread dedicada.
"""
import asyncio
import base64
import json
import logging
import queue
import sys
import threading
import traceback
from typing import Callable

from aiohttp import web

# ─── libfprint via GI ────────────────────────────────────────────────────────
try:
    import gi
    gi.require_version('FPrint', '2.0')
    from gi.repository import FPrint, GLib, Gio
    FPRINT_AVAILABLE = True
except (ImportError, ValueError) as e:
    FPRINT_AVAILABLE = False
    print(f"[AVISO] libfprint-2 indisponível: {e}", file=sys.stderr)

logging.basicConfig(level=logging.DEBUG,
                    format='%(asctime)s [%(levelname)s] %(message)s',
                    datefmt='%H:%M:%S')
log = logging.getLogger('bridge')

PORT = 7001
_enroll_stages = 4
CORS = {"Access-Control-Allow-Origin": "*"}


# ─── Thread dedicada para libfprint ──────────────────────────────────────────
# GLib/libfprint NÃO é thread-safe. Todos os objetos FPrint devem ser criados
# e usados SEMPRE na mesma thread (a fprint_thread abaixo).
# Comunicação com asyncio via _job_queue (submissão) e future.set_result (retorno).

_job_queue: queue.Queue = queue.Queue()  # Fila de (fn, args, future, loop)
_fprint_thread: threading.Thread | None = None
_dev = None
_connected = False
_enroll_stages = 4  # Atualizado dinamicamente após abrir o dispositivo


def _fprint_worker():
    """
    Thread dedicada ao libfprint. Processa jobs da fila sequencialmente.
    Roda em loop infinito até receber None como job.
    """
    global _dev, _connected

    # Abre o dispositivo nesta thread (NUNCA em outra)
    if FPRINT_AVAILABLE:
        try:
            ctx = FPrint.Context()
            devices = ctx.get_devices()
            if devices:
                dev = devices[0]
                dev.open_sync(None)
                _dev = dev
                _connected = True
                _enroll_stages = dev.get_nr_enroll_stages()
                log.info("Leitor aberto: %s (%d amostras)", dev.get_name(), _enroll_stages)
            else:
                log.warning("Nenhum leitor encontrado.")
        except Exception as exc:
            log.error("Erro ao abrir leitor: %s", exc)

    # Processa jobs
    while True:
        job = _job_queue.get()
        if job is None:
            break  # Sinal de encerramento
        fn, args, fut, loop = job
        try:
            result = fn(*args)
            loop.call_soon_threadsafe(fut.set_result, result)
        except Exception as exc:
            loop.call_soon_threadsafe(fut.set_exception, exc)

    # Fecha dispositivo ao encerrar
    if _dev:
        try:
            _dev.close_sync(None)
        except Exception:
            pass


def _submit(fn, *args) -> asyncio.Future:
    """Submete fn(*args) para executar na fprint_thread. Retorna Future asyncio."""
    loop = asyncio.get_running_loop()
    fut = loop.create_future()
    _job_queue.put((fn, args, fut, loop))
    return fut


def _start_fprint_thread():
    global _fprint_thread
    _fprint_thread = threading.Thread(target=_fprint_worker, name='fprint', daemon=True)
    _fprint_thread.start()


def _stop_fprint_thread():
    _job_queue.put(None)
    if _fprint_thread:
        _fprint_thread.join(timeout=3)


# ─── Operações libfprint (executam na fprint_thread via _submit) ──────────────

class _Cancelled(Exception):
    pass


def _do_enroll(progress_cb: Callable[[str], None], cancel_evt: threading.Event) -> bytes:
    """
    Enroll usando a API async do libfprint-2 + GLib.MainLoop.
    A API async é a correta para libfprint-2: usa callbacks GLib que
    precisam de um MainLoop rodando para serem processados.
    """
    dev = _dev
    if dev is None:
        raise RuntimeError("Leitor não conectado")

    ctx = GLib.MainContext.new()
    loop = GLib.MainLoop.new(ctx, False)
    cancellable = Gio.Cancellable()
    result_holder: list = [None]   # [bytes] ou [Exception]

    def watcher():
        cancel_evt.wait()
        GLib.idle_add(cancellable.cancel)
    threading.Thread(target=watcher, daemon=True).start()

    # user_data deve ser tupla não-vazia para PyGObject passar o argumento
    def on_progress(device, completed, print_, error, _data):
        if error:
            log.debug("on_progress erro: %s", error)
            return
        if completed < _enroll_stages:
            progress_cb(f"Amostra {completed} de {_enroll_stages} — "
                        "retire o dedo e coloque novamente")
        else:
            progress_cb("Gerando template biométrico...")

    def on_done(device, async_result, _data):
        log.debug("on_done chamado")
        try:
            enrolled = device.enroll_finish(async_result)
            raw = enrolled.serialize()
            log.debug("serialize() type=%s  is_GVariant=%s  is_bytes=%s",
                      type(raw).__name__,
                      isinstance(raw, GLib.Variant),
                      isinstance(raw, bytes))
            # serialize() retorna bytes (PyGObject converte GVariant<ay> → bytes)
            result_holder[0] = raw if isinstance(raw, bytes) else bytes(raw)
            log.debug("Template gerado: %d bytes", len(result_holder[0]))
        except GLib.Error as exc:
            log.error("on_done GLib.Error: %s", exc.message)
            if cancellable.is_cancelled() or cancel_evt.is_set():
                result_holder[0] = _Cancelled()
            else:
                result_holder[0] = RuntimeError(exc.message)
        except Exception as exc:
            log.error("on_done Exception: %s", exc)
            result_holder[0] = exc
        finally:
            loop.quit()

    progress_cb(f"Coloque o dedo no leitor ({_enroll_stages} amostras)...")

    template = FPrint.Print.new(dev)
    ctx.push_thread_default()
    try:
        dev.enroll(template, cancellable, on_progress, (None,), on_done, (None,))
        loop.run()
    finally:
        ctx.pop_thread_default()

    r = result_holder[0]
    if isinstance(r, _Cancelled):
        raise r
    if isinstance(r, Exception):
        raise r
    return r


def _do_verify(progress_cb: Callable[[str], None], cancel_evt: threading.Event) -> bytes:
    """Captura 1 digital para assinatura usando API async + GLib.MainLoop."""
    dev = _dev
    if dev is None:
        raise RuntimeError("Leitor não conectado")

    ctx = GLib.MainContext.new()
    loop = GLib.MainLoop.new(ctx, False)
    cancellable = Gio.Cancellable()
    result_holder: list = [None]

    def watcher():
        cancel_evt.wait()
        GLib.idle_add(cancellable.cancel)
    threading.Thread(target=watcher, daemon=True).start()

    def on_done(device, async_result, _data):
        log.debug("on_done (verify) chamado")
        try:
            captured = device.enroll_finish(async_result)
            raw = captured.serialize()
            result_holder[0] = raw if isinstance(raw, bytes) else bytes(raw)
        except GLib.Error as exc:
            log.error("on_done (verify) GLib.Error: %s", exc.message)
            if cancellable.is_cancelled() or cancel_evt.is_set():
                result_holder[0] = _Cancelled()
            else:
                result_holder[0] = RuntimeError(exc.message)
        except Exception as exc:
            result_holder[0] = exc
        finally:
            loop.quit()

    progress_cb("Coloque o dedo no leitor para assinar...")
    template = FPrint.Print.new(dev)
    ctx.push_thread_default()
    try:
        dev.enroll(template, cancellable, None, None, on_done, (None,))
        loop.run()
    finally:
        ctx.pop_thread_default()

    r = result_holder[0]
    if isinstance(r, _Cancelled):
        raise r
    if isinstance(r, Exception):
        raise r
    return r


# ─── Identificação 1:N (matching contra templates do banco) ──────────────────

BACKEND_URL = "http://localhost:3000/api"

def _do_identify(progress_cb: Callable[[str], None], cancel_evt: threading.Event,
                 gallery: list) -> str | None:
    """
    Identificação 1:N: captura uma digital e compara contra gallery.
    gallery = lista de (employee_id, FPrint.Print)
    Retorna employee_id do match ou None.
    Usa dev.identify() do libfprint.
    """
    dev = _dev
    if dev is None:
        raise RuntimeError("Leitor não conectado")
    if not gallery:
        raise RuntimeError("Nenhum template cadastrado no sistema")

    prints = [fp for _, fp in gallery]

    ctx = GLib.MainContext.new()
    loop = GLib.MainLoop.new(ctx, False)
    cancellable = Gio.Cancellable()
    result_holder: list = [None]

    def watcher():
        cancel_evt.wait()
        GLib.idle_add(cancellable.cancel)
    threading.Thread(target=watcher, daemon=True).start()

    def on_done(device, async_result, _data):
        log.debug("identify on_done chamado")
        try:
            # identify_finish retorna (matched_gallery_print, scanned_print)
            matched_print, _scanned = device.identify_finish(async_result)
            if matched_print is not None:
                # Busca employee_id pelo objeto matched (identidade de ponteiro)
                emp_id = next((eid for eid, fp in gallery if fp is matched_print), None)
                result_holder[0] = emp_id
                log.info("Identificado: employee_id=%s", emp_id)
            else:
                result_holder[0] = None
                log.info("Nenhum match encontrado")
        except GLib.Error as exc:
            log.error("identify on_done erro: %s", exc.message)
            if cancellable.is_cancelled() or cancel_evt.is_set():
                result_holder[0] = _Cancelled()
            else:
                result_holder[0] = RuntimeError(exc.message)
        except Exception as exc:
            log.error("identify on_done exc: %s", exc)
            result_holder[0] = exc
        finally:
            loop.quit()

    progress_cb("Coloque o dedo no leitor para identificação...")
    ctx.push_thread_default()
    try:
        dev.identify(prints, cancellable, None, (), on_done, (None,))
        loop.run()
    finally:
        ctx.pop_thread_default()

    r = result_holder[0]
    if isinstance(r, _Cancelled):
        raise r
    if isinstance(r, Exception):
        raise r
    return r  # employee_id string ou None


async def _fetch_gallery() -> list:
    """Busca templates do backend e deserializa como FPrint.Print."""
    import urllib.request
    import urllib.error

    url = f"{BACKEND_URL}/employees/biometric-templates"
    try:
        with urllib.request.urlopen(url, timeout=5) as resp:
            data = json.loads(resp.read())
    except Exception as exc:
        raise RuntimeError(f"Falha ao buscar templates do backend: {exc}")

    gallery = []
    for item in data:
        raw = base64.b64decode(item["templateBase64"])
        try:
            # PyGObject converte GVariant<ay> → Python bytes automaticamente.
            # Para reconstruir, criamos GVariant("ay", raw) diretamente.
            variant = GLib.Variant("ay", list(raw))
            fp = FPrint.Print.deserialize(variant)
            log.debug("Deserializado template %s OK", item.get("employeeId"))
        except Exception as exc:
            log.warning("Falha ao deserializar template %s: %s — recadastro necessário",
                        item.get("employeeId"), exc)
            continue
        gallery.append((item["employeeId"], fp))

    log.info("Gallery carregada: %d templates", len(gallery))
    return gallery


# ─── Simulação ────────────────────────────────────────────────────────────────

async def _simulate_enroll(send_msg: Callable):
    send_msg(f"[SIM] Coloque o dedo ({_enroll_stages} amostras)...")
    for i in range(1, _enroll_stages + 1):
        await asyncio.sleep(1.5)
        if i < _enroll_stages:
            send_msg(f"[SIM] Amostra {i} de {_enroll_stages} — retire e recoloque")
        else:
            send_msg("[SIM] Gerando template...")
    await asyncio.sleep(0.5)
    return b'\x00FAKE_TEMPLATE' + b'\xAB' * 64


async def _simulate_verify(send_msg: Callable):
    send_msg("[SIM] Coloque o dedo para assinar...")
    await asyncio.sleep(2)
    send_msg("[SIM] Digital capturada!")
    return b'\x00FAKE_SAMPLE' + b'\xCD' * 32


# ─── WebSocket handler ────────────────────────────────────────────────────────

async def ws_handler(request: web.Request) -> web.WebSocketResponse:
    ws = web.WebSocketResponse()
    await ws.prepare(request)
    log.info("WS conectado: %s", request.remote)

    loop = asyncio.get_running_loop()
    cancel_evt = threading.Event()
    active_task: asyncio.Task | None = None
    msg_q: asyncio.Queue[str] = asyncio.Queue()

    async def progress_sender():
        while not ws.closed:
            try:
                text = await asyncio.wait_for(msg_q.get(), timeout=0.5)
                await ws.send_json({"type": "progress", "message": text})
            except asyncio.TimeoutError:
                pass
            except Exception:
                break

    sender = asyncio.create_task(progress_sender())

    def on_progress(text: str):
        asyncio.run_coroutine_threadsafe(msg_q.put(text), loop)

    async def do_capture(is_enroll: bool):
        try:
            if _connected:
                fn = _do_enroll if is_enroll else _do_verify
                result_bytes = await _submit(fn, on_progress, cancel_evt)
            else:
                sim = _simulate_enroll if is_enroll else _simulate_verify
                result_bytes = await sim(on_progress)

            b64 = base64.b64encode(result_bytes).decode()
            rtype = "enroll_complete" if is_enroll else "verify_complete"
            key = "templateBase64" if is_enroll else "sampleBase64"
            # Aguarda a fila de progresso ser drenada antes de enviar resultado
            await asyncio.sleep(0.1)
            log.info("Enviando %s (%d bytes b64)", rtype, len(b64))
            if not ws.closed:
                await ws.send_json({"type": rtype, key: b64,
                                    "message": "Concluído!"})
                log.info("%s enviado com sucesso", rtype)
        except _Cancelled:
            if not ws.closed:
                await ws.send_json({"type": "cancelled",
                                    "message": "Operação cancelada"})
        except asyncio.CancelledError:
            if not ws.closed:
                await ws.send_json({"type": "cancelled",
                                    "message": "Operação cancelada"})
        except Exception as exc:
            log.error("Erro captura: %s\n%s", exc, traceback.format_exc())
            if not ws.closed:
                await ws.send_json({"type": "error", "message": str(exc)})

    try:
        async for msg in ws:
            if msg.type == web.WSMsgType.TEXT:
                try:
                    data = json.loads(msg.data)
                    cmd = data.get("command", "")
                    log.debug("CMD: %s", cmd)
                except json.JSONDecodeError:
                    await ws.send_json({"type": "error", "message": "JSON inválido"})
                    continue

                if cmd == "status":
                    await ws.send_json({
                        "type": "status", "online": True,
                        "sdkAvailable": FPRINT_AVAILABLE,
                        "readerConnected": _connected,
                        "platform": "linux",
                        "driver": "libfprint-2" if FPRINT_AVAILABLE else "simulation",
                    })

                elif cmd == "capture_identify":
                    if active_task and not active_task.done():
                        await ws.send_json({"type": "error", "message": "Operação em andamento"})
                        continue
                    cancel_evt.clear()

                    async def do_identify():
                        try:
                            await ws.send_json({"type": "progress",
                                                "message": "Buscando templates cadastrados..."})
                            if _connected:
                                gallery = await _fetch_gallery()
                                employee_id = await _submit(_do_identify, on_progress,
                                                            cancel_evt, gallery)
                            else:
                                await asyncio.sleep(2)
                                employee_id = None  # Simulação: nunca identifica

                            if employee_id:
                                await ws.send_json({"type": "identify_complete",
                                                    "employeeId": employee_id})
                            else:
                                await ws.send_json({"type": "identify_failed",
                                                    "message": "Funcionário não identificado. "
                                                               "Verifique o cadastro biométrico."})
                        except _Cancelled:
                            await ws.send_json({"type": "cancelled",
                                                "message": "Operação cancelada"})
                        except asyncio.CancelledError:
                            await ws.send_json({"type": "cancelled",
                                                "message": "Operação cancelada"})
                        except Exception as exc:
                            log.error("Erro identify: %s\n%s", exc, traceback.format_exc())
                            await ws.send_json({"type": "error", "message": str(exc)})

                    active_task = asyncio.create_task(do_identify())

                elif cmd in ("capture_enroll", "capture_verify"):
                    if active_task and not active_task.done():
                        await ws.send_json({"type": "error",
                                            "message": "Operação em andamento"})
                        continue
                    cancel_evt.clear()
                    active_task = asyncio.create_task(
                        do_capture(cmd == "capture_enroll")
                    )

                elif cmd == "cancel":
                    cancel_evt.set()
                    if active_task and not active_task.done():
                        active_task.cancel()
                        try:
                            await active_task
                        except asyncio.CancelledError:
                            pass
                    await ws.send_json({"type": "cancelled",
                                        "message": "Cancelado"})
                else:
                    await ws.send_json({"type": "error",
                                        "message": f"Comando desconhecido: {cmd}"})

            elif msg.type in (web.WSMsgType.ERROR, web.WSMsgType.CLOSE):
                break

    except Exception as exc:
        log.error("Erro WS handler: %s\n%s", exc, traceback.format_exc())
    finally:
        cancel_evt.set()
        sender.cancel()
        if active_task and not active_task.done():
            active_task.cancel()
        log.info("WS desconectado: %s", request.remote)

    return ws


# ─── HTTP /status ─────────────────────────────────────────────────────────────

async def status_handler(request: web.Request) -> web.Response:
    if request.method == "OPTIONS":
        return web.Response(headers=CORS)
    return web.json_response({
        "online": True,
        "readerConnected": _connected,
        "sdkAvailable": FPRINT_AVAILABLE,
        "platform": "linux",
        "driver": "libfprint-2" if FPRINT_AVAILABLE else "simulation",
    }, headers=CORS)


# ─── Main ─────────────────────────────────────────────────────────────────────

async def main():
    log.info("=== Biometric Bridge (Linux / libfprint-2) ===")

    # Inicia thread dedicada ao libfprint (abre o dispositivo lá dentro)
    _start_fprint_thread()

    # Aguarda thread inicializar o dispositivo (~1s)
    await asyncio.sleep(1.2)

    if _connected:
        log.info("Leitor pronto.")
    else:
        log.warning("Leitor não disponível — modo SIMULAÇÃO.")

    app = web.Application()
    app.router.add_get('/ws', ws_handler)
    app.router.add_get('/status', status_handler)
    app.router.add_route('OPTIONS', '/status', status_handler)

    runner = web.AppRunner(app)
    await runner.setup()
    site = web.TCPSite(runner, '0.0.0.0', PORT)
    await site.start()

    log.info("HTTP : http://localhost:%d/status", PORT)
    log.info("WS   : ws://localhost:%d/ws", PORT)

    try:
        await asyncio.Event().wait()
    except asyncio.CancelledError:
        pass
    finally:
        _stop_fprint_thread()
        await runner.cleanup()


if __name__ == '__main__':
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        log.info("Bridge encerrado.")
