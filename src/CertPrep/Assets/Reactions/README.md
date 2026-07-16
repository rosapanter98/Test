# Reaction assets

`gremlin-reactions-source.png` is an original image generated with OpenAI's built-in image-generation tool for this project. It is not based on an existing meme character.

The source prompt requested a four-panel, graphite-black and electric-blue 3D cartoon sprite sheet showing the same office gremlin failing, confused, barely passing, and celebrating. It explicitly excluded text, logos, watermarks, copyrighted characters, and protected-group stereotypes.

Rebuild the four local looping GIFs with the bundled Codex Python runtime and Pillow:

```powershell
& '<codex-python-path>' tools/build-reaction-gifs.py `
  src/CertPrep/Assets/Reactions/gremlin-reactions-source.png `
  src/CertPrep/Assets/Reactions
```

Only the generated `.gif` files are packaged as Avalonia resources. The source sheet and build script remain development inputs.
