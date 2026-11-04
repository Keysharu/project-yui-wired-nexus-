import os

p = r"C:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets\Scripts\Companion\NyxBillboardAnimator.cs"
with open(p, "r", encoding="utf-8") as f:
    text = f.read()

# 1. Add turn360Sprites field
old_field = "[SerializeField] private Sprite viewBackSprite;"
new_field = """[SerializeField] private Sprite viewBackSprite;
        [Tooltip("8 vistas rotatorias a 45 grados (turn360_0 a turn360_7) para renderizado 2.5D HD")]
        [SerializeField] private Sprite[] turn360Sprites;"""
if old_field in text and "turn360Sprites" not in text:
    text = text.replace(old_field, new_field, 1)

# 2. Add auto-loading in Awake
old_awake = "bodySpriteRenderer = GetComponentInChildren<SpriteRenderer>();\n            }"
if old_awake not in text:
    old_awake = "bodySpriteRenderer = GetComponentInChildren<SpriteRenderer>();\r\n            }"

new_awake = """bodySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (turn360Sprites == null || turn360Sprites.Length < 8 || turn360Sprites[0] == null)
            {
                turn360Sprites = new Sprite[8];
                for (int i = 0; i < 8; i++)
                {
                    turn360Sprites[i] = Resources.Load<Sprite>("Sprites/Nyx/turn360_" + i);
                }
            }"""
if old_awake in text and 'turn360Sprites = new Sprite[8]' not in text:
    text = text.replace(old_awake, new_awake, 1)

# 3. Update sprite selection with 8-way directional calculation
old_idle_unix = """            // 5. Sprite base de reposo
            if (idleSprite != null)
            {
                bodySpriteRenderer.sprite = idleSprite;
            }
            else if (viewFrontSprite != null)
            {
                bodySpriteRenderer.sprite = viewFrontSprite;
            }"""

old_idle_win = old_idle_unix.replace("\n", "\r\n")

new_idle = """            // 5. Sistema 2.5D HD de 8 Ángulos Direccionales
            if (turn360Sprites != null && turn360Sprites.Length == 8 && turn360Sprites[0] != null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 toCam = cam.transform.position - transform.position;
                    toCam.y = 0;
                    if (toCam.sqrMagnitude > 0.001f)
                    {
                        toCam.Normalize();
                        Vector3 fwd = transform.forward;
                        fwd.y = 0;
                        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
                        fwd.Normalize();

                        float signedAngle = Vector3.SignedAngle(fwd, toCam, Vector3.up);
                        float angle = (signedAngle + 360f) % 360f;
                        int sector = Mathf.RoundToInt(angle / 45f) % 8;

                        if (turn360Sprites[sector] != null)
                        {
                            bodySpriteRenderer.sprite = turn360Sprites[sector];
                            return;
                        }
                    }
                }
            }

            // Fallback a idleSprite
            if (idleSprite != null)
            {
                bodySpriteRenderer.sprite = idleSprite;
            }
            else if (viewFrontSprite != null)
            {
                bodySpriteRenderer.sprite = viewFrontSprite;
            }"""

if old_idle_win in text:
    text = text.replace(old_idle_win, new_idle, 1)
elif old_idle_unix in text:
    text = text.replace(old_idle_unix, new_idle, 1)

with open(p, "w", encoding="utf-8") as f:
    f.write(text)

print("Patch applied successfully to NyxBillboardAnimator.cs")
