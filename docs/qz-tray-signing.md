# Assinatura QZ Tray

O QZ Tray usa assinatura por mensagem para liberar impressao silenciosa.
Nesta etapa o projeto usa o Demo Cert gerado no QZ Tray da estacao de
impressao.

## Local

1. No QZ Tray, gere os arquivos pelo Site Manager:
   - `digital-certificate.txt`
   - `private-key.pem`
2. Copie `config.local.example.ps1` para `config.local.ps1`.
3. Preencha:
   - `QzCertificatePath`
   - `QzPrivateKeyPath`
4. Execute:

```powershell
.\scripts\configurar-ambiente-local.ps1
```

O script le os arquivos e grava os conteudos em User Secrets:

- `QzSigning:Certificate`
- `QzSigning:PrivateKey`

Nao coloque a private key no frontend, em assets, no banco, em logs ou no Git.

## Render

Para testar em producao com a estacao atual, configure as variaveis de ambiente
da API no Render:

- `QzSigning__Certificate`: conteudo completo de `digital-certificate.txt`
- `QzSigning__PrivateKey`: conteudo completo de `private-key.pem`

Depois reinicie a API.

## Troca no PC do restaurante

Demo Certs funcionam apenas na instalacao do QZ Tray onde foram gerados. No PC
final do restaurante:

1. Instale o QZ Tray.
2. Gere um novo Demo Cert pelo Site Manager.
3. Atualize `QzSigning__Certificate` e `QzSigning__PrivateKey` no Render.
4. Reinicie a API.
5. Entre no admin e clique em `Imprimir teste`.

Nesta etapa existe uma unica estacao de impressao. Nao ha suporte simultaneo a
varios Demo Certs.
