import os

p = r"C:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets\Scripts\Core\CyberVisualAndCombatUpgrader.cs"
with open(p, "r", encoding="utf-8") as f:
    text = f.read()

cafe_code = '''
        /// <summary>
        /// Construye y amuebla el interior explorable del Cyber Cafe Armeria
        /// con vitrinas de cristal para armamento, mostrador Lounge y terminales.
        /// </summary>
        public void BuildCyberCafeArmoryInterior()
        {
            GameObject vrRoot = GameObject.Find("FullVR_MegacitySkyline_Root");
            if (vrRoot == null) return;
            if (vrRoot.transform.Find("CyberCafe_Armory_Interior") != null) return;

            GameObject cafeRoom = new GameObject("CyberCafe_Armory_Interior");
            cafeRoom.transform.SetParent(vrRoot.transform, false);
            // Emplazado en el interior de la fachada del cafe (Acera Izquierda)
            cafeRoom.transform.localPosition = new Vector3(-6.5f, 0.0f, -3.5f);

            // 1. Suelo pulido con reflejos
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Armory_Interior_Floor";
            floor.transform.SetParent(cafeRoom.transform, false);
            floor.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            floor.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
            floor.GetComponent<MeshRenderer>().material = matWetAsphalt;

            // 2. Vitrina central para armamento
            GameObject vitrine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vitrine.name = "Armory_Central_Glass_Vitrine";
            vitrine.transform.SetParent(cafeRoom.transform, false);
            vitrine.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            vitrine.transform.localScale = new Vector3(1.4f, 1.8f, 0.8f);
            vitrine.GetComponent<MeshRenderer>().material = matDarkMetal;

            // Luz cenital cian en la vitrina
            GameObject vitrineLight = new GameObject("Vitrine_SpotLight");
            vitrineLight.transform.SetParent(vitrine.transform, false);
            vitrineLight.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            Light l = vitrineLight.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.0f, 1.0f, 0.88f);
            l.range = 4f;
            l.intensity = 2.5f;

            // 3. Mostrador de recepcion / Lounge
            GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Lounge_Service_Counter";
            counter.transform.SetParent(cafeRoom.transform, false);
            counter.transform.localPosition = new Vector3(0f, 0.6f, 2.2f);
            counter.transform.localScale = new Vector3(2.5f, 1.1f, 0.6f);
            counter.GetComponent<MeshRenderer>().material = matDarkMetal;

            // Tira neon en el mostrador
            GameObject counterGlow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterGlow.name = "Counter_Neon_Trim";
            counterGlow.transform.SetParent(counter.transform, false);
            counterGlow.transform.localPosition = new Vector3(0f, 0.45f, 0.31f);
            counterGlow.transform.localScale = new Vector3(1.0f, 0.08f, 0.05f);
            counterGlow.GetComponent<MeshRenderer>().material = matNeonCyan;

            Debug.Log("[CyberVisualAndCombatUpgrader] Interior del Cyber Cafe Armeria construido con exito.");
        }
'''

if "BuildCyberCafeArmoryInterior" not in text:
    last_brace = text.rfind('}')
    text = text[:last_brace] + cafe_code + '\n' + text[last_brace:]
    text = text.replace("UpgradeExistingBossInstance();", "UpgradeExistingBossInstance();\n            BuildCyberCafeArmoryInterior();")

with open(p, "w", encoding="utf-8") as f:
    f.write(text)

print("Applied cafe interior patch successfully")
