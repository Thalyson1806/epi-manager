#!/bin/bash
# Instala regra udev para o leitor DigitalPersona U.are.U 4000/4500
# Permite que usuários do grupo 'plugdev' acessem o leitor sem sudo.
#
# Uso: sudo ./setup_udev.sh

set -e

RULE='SUBSYSTEM=="usb", ATTRS{idVendor}=="05ba", ATTRS{idProduct}=="000a", GROUP="plugdev", MODE="0664"'
RULE_FILE="/etc/udev/rules.d/99-digitalpersona.rules"

echo "Instalando regra udev para DigitalPersona U.are.U..."
echo "$RULE" > "$RULE_FILE"
echo "Regra criada em $RULE_FILE"

udevadm control --reload-rules
udevadm trigger

echo ""
echo "Pronto! Desplugue e replugue o leitor USB."
echo "Depois rode: python3 biometric_bridge.py"
