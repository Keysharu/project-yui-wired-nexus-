import os

p = r"C:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets\Scripts\Gameplay\MRUKEncounterSpawner.cs"
with open(p, "r", encoding="utf-8") as f:
    text = f.read()

old_block = """                case EncounterPhase.LootSupplyChests:
                    phaseTimer += Time.deltaTime;
                    if (!sequenceStarted && phaseTimer >= autoOpenChestsFallbackSeconds)
                    {
                        ForceOpenAllChests();
                    }
                    break;"""

new_block = """                case EncounterPhase.LootSupplyChests:
                    // En modo VR Inmersivo, no forzar la apertura de cofres para permitir explorar la calle tranquilamente
                    var roomDec = FindAnyObjectByType<ProjectNyx.Core.CyberRoomDecorator3D>();
                    if (roomDec != null && roomDec.IsFullVrActive)
                    {
                        phaseTimer = 0f;
                        break;
                    }

                    phaseTimer += Time.deltaTime;
                    if (!sequenceStarted && phaseTimer >= autoOpenChestsFallbackSeconds)
                    {
                        ForceOpenAllChests();
                    }
                    break;"""

old_win = old_block.replace("\n", "\r\n")
new_win = new_block.replace("\n", "\r\n")

if old_win in text:
    text = text.replace(old_win, new_win, 1)
    print("Replaced with Windows CRLF")
elif old_block in text:
    text = text.replace(old_block, new_block, 1)
    print("Replaced with Unix LF")
else:
    print("Block not found!")

with open(p, "w", encoding="utf-8") as f:
    f.write(text)
