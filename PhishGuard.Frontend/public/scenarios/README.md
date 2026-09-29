# Assets dos cenários

- `mercado-livre-logo.png`: logo de aperto de mãos em português, obtida do asset público https://http2.mlstatic.com/ui/navigation/2.3.5/mercadolibre/logo-pt__large@2x.png. Cópia idêntica incorporada no backend como `Resources/EmailAssets/mercado-liv-logo.png`, mantendo o CID `logo-mercadoliv`.
- `microsoft-background-mobile.webp`: referência fornecida pelo usuário, convertida para WebP preservando suas dimensões (1020 × 978).
- `microsoft-background-wide.webp`: extensão da referência gerada com a ferramenta integrada de imagens, depois convertida para WebP (1672 × 941). O CSS usa `cover` e seleciona a versão móvel em telas estreitas.

Prompt usado para a extensão:

> Edit the attached abstract background for a responsive website login. Extend the canvas horizontally to a wide 16:9 landscape composition, preferably 2560x1440. Preserve its extremely pale pastel blue/lavender/peach/off-white palette, soft glass-like curved planes, the fine diagonal curved edge rising from the bottom left, and gentle lighting. Extend the existing shapes naturally into the wider sides; do not stretch or distort them. Keep the central area quiet and almost white so a white login card remains readable. Smooth gradients, high quality clean edges, very low contrast matching input. No text, no logo, no UI card, no objects, no watermark. The input is the edit target; output only the extended background asset.
