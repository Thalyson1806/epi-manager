// =============================================================================
// useBiometric — Hook React para comunicação com o Biometric Bridge
// =============================================================================
// Este hook gerencia a conexão WebSocket com o serviço local (biometric-bridge)
// que roda na máquina Windows com o leitor DigitalPersona conectado.
//
// FLUXO DE USO:
//   const bio = useBiometric()
//
//   // Verificar se bridge está online
//   await bio.checkStatus()  →  { online, sdkAvailable, readerConnected }
//
//   // Cadastrar biometria (enrollment - 4 amostras)
//   const templateBase64 = await bio.captureEnrollment(msg => setProgress(msg))
//
//   // Capturar para identificação (verification - 1 amostra)
//   const sampleBase64 = await bio.captureVerification(msg => setProgress(msg))
//
// O bridge deve estar rodando em ws://localhost:7001/ws
// Se o bridge não estiver disponível, as funções lançam erro com mensagem clara.
// =============================================================================

import { useCallback, useRef } from 'react'

// Endereço local do biometric bridge
// Altere a porta se necessário (configurável no biometric-bridge/Program.cs)
const BRIDGE_URL = 'ws://localhost:7001/ws'
const BRIDGE_STATUS_URL = 'http://localhost:7001/status'

export interface BridgeStatus {
  online: boolean
  sdkAvailable: boolean
  readerConnected: boolean
}

export function useBiometric() {
  const wsRef = useRef<WebSocket | null>(null)

  // --------------------------------------------------------------------------
  // checkStatus: verifica se o bridge está rodando
  // Chame antes de qualquer operação biométrica para dar feedback ao usuário
  // --------------------------------------------------------------------------
  const checkStatus = useCallback(async (): Promise<BridgeStatus> => {
    try {
      const response = await fetch(BRIDGE_STATUS_URL, { signal: AbortSignal.timeout(2000) })
      if (!response.ok) throw new Error('Bridge offline')
      const data = await response.json()
      return {
        online: true,
        sdkAvailable: data.sdkAvailable,
        readerConnected: data.readerConnected,
      }
    } catch {
      return { online: false, sdkAvailable: false, readerConnected: false }
    }
  }, [])

  // --------------------------------------------------------------------------
  // Função interna: abre WebSocket e executa um comando
  // Resolve com o resultado quando receber "enroll_complete" ou "verify_complete"
  // --------------------------------------------------------------------------
  const executeCommand = useCallback(
    (command: string, onProgress: (msg: string) => void): Promise<string> => {
      return new Promise((resolve, reject) => {
        // Fecha conexão anterior se existir
        if (wsRef.current) {
          wsRef.current.close()
        }

        const ws = new WebSocket(BRIDGE_URL)
        wsRef.current = ws

        // Timeout de 60 segundos para toda a operação
        const timeout = setTimeout(() => {
          ws.close()
          reject(new Error('Timeout: nenhuma resposta do leitor em 60 segundos'))
        }, 60000)

        ws.onopen = () => {
          // Envia o comando assim que a conexão estiver aberta
          ws.send(JSON.stringify({ command }))
        }

        ws.onmessage = (event) => {
          const data = JSON.parse(event.data)

          switch (data.type) {
            // Mensagem de progresso: ex "Amostra 1 de 4"
            case 'progress':
              onProgress(data.message)
              break

            // Enrollment concluído: retorna template base64
            case 'enroll_complete':
              clearTimeout(timeout)
              ws.close()
              resolve(data.templateBase64)
              break

            // Verificação concluída: retorna amostra base64
            case 'verify_complete':
              clearTimeout(timeout)
              ws.close()
              resolve(data.sampleBase64)
              break

            // Operação cancelada
            case 'cancelled':
              clearTimeout(timeout)
              ws.close()
              reject(new Error('Operação cancelada'))
              break

            // Erro do bridge
            case 'error':
              clearTimeout(timeout)
              ws.close()
              reject(new Error(data.message))
              break
          }
        }

        ws.onerror = () => {
          clearTimeout(timeout)
          reject(
            new Error(
              'Biometric Bridge não encontrado em localhost:7001.\n' +
              'Verifique se o serviço está rodando na máquina com o leitor conectado.',
            ),
          )
        }

        ws.onclose = (event) => {
          clearTimeout(timeout)
          // Se fechou sem resolver, é um erro
          if (event.code !== 1000) {
            reject(new Error('Conexão com o bridge foi encerrada inesperadamente'))
          }
        }
      })
    },
    [],
  )

  // --------------------------------------------------------------------------
  // captureEnrollment: captura 4 amostras e gera template biométrico
  // Usado no cadastro de funcionário (tela de RH)
  // Retorna: templateBase64 para salvar via POST /api/employees/{id}/biometric
  // --------------------------------------------------------------------------
  const captureEnrollment = useCallback(
    (onProgress: (msg: string) => void) =>
      executeCommand('capture_enroll', onProgress),
    [executeCommand],
  )

  // --------------------------------------------------------------------------
  // captureVerification: captura 1 amostra para assinatura da ficha
  // Usado no Passo 3 da Entrega de EPI (assinatura)
  // --------------------------------------------------------------------------
  const captureVerification = useCallback(
    (onProgress: (msg: string) => void) =>
      executeCommand('capture_verify', onProgress),
    [executeCommand],
  )

  // --------------------------------------------------------------------------
  // captureIdentification: identificação 1:N via bridge
  // Bridge busca templates do backend, usa libfprint identify, retorna employeeId
  // Retorna: employeeId (string) do funcionário identificado
  // Lança erro se não identificado
  // --------------------------------------------------------------------------
  const captureIdentification = useCallback(
    (onProgress: (msg: string) => void): Promise<string> => {
      return new Promise((resolve, reject) => {
        if (wsRef.current) wsRef.current.close()

        const ws = new WebSocket(BRIDGE_URL)
        wsRef.current = ws

        const timeout = setTimeout(() => {
          ws.close()
          reject(new Error('Timeout: nenhuma resposta do leitor em 60 segundos'))
        }, 60000)

        ws.onopen = () => ws.send(JSON.stringify({ command: 'capture_identify' }))

        ws.onmessage = (event) => {
          const data = JSON.parse(event.data)
          switch (data.type) {
            case 'progress':
              onProgress(data.message)
              break
            case 'identify_complete':
              clearTimeout(timeout)
              ws.close()
              resolve(data.employeeId)
              break
            case 'identify_failed':
              clearTimeout(timeout)
              ws.close()
              reject(new Error(data.message))
              break
            case 'cancelled':
              clearTimeout(timeout)
              ws.close()
              reject(new Error('Operação cancelada'))
              break
            case 'error':
              clearTimeout(timeout)
              ws.close()
              reject(new Error(data.message))
              break
          }
        }

        ws.onerror = () => {
          clearTimeout(timeout)
          reject(new Error('Biometric Bridge não encontrado em localhost:7001.'))
        }

        ws.onclose = (event) => {
          clearTimeout(timeout)
          if (event.code !== 1000) {
            reject(new Error('Conexão com o bridge foi encerrada inesperadamente'))
          }
        }
      })
    },
    [],
  )

  // --------------------------------------------------------------------------
  // cancel: cancela operação em andamento
  // --------------------------------------------------------------------------
  const cancel = useCallback(() => {
    if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
      wsRef.current.send(JSON.stringify({ command: 'cancel' }))
    }
  }, [])

  return { checkStatus, captureEnrollment, captureVerification, captureIdentification, cancel }
}
