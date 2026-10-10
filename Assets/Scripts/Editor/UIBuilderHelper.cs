using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class UIBuilderHelper
{
    [MenuItem("Tools/Paws & Seeds/Crear Panel Estilizado (Pergamino)", false, 10)]
    public static void CreateStyledPanel()
    {
        // 1. Buscar o crear Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
        }

        // 2. Crear Objeto Raíz del Panel
        GameObject panelRoot = new GameObject("Panel_Estilizado", typeof(RectTransform), typeof(Image));
        panelRoot.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 580);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panelRoot.GetComponent<Image>();
        Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/UI_Panel_Parchment.png");
        if (panelSprite != null)
        {
            panelImage.sprite = panelSprite;
            panelImage.type = Image.Type.Simple;
            panelImage.preserveAspect = true;
        }

        // 3. Título del Panel (Ubicado en el cartel superior de madera)
        GameObject titleGO = new GameObject("Text_Titulo", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(panelRoot.transform, false);
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -18);
        titleRect.sizeDelta = new Vector2(360, 48);

        TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "TITULO";
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontSize = 28;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = new Color(0.96f, 0.86f, 0.62f); // Dorado cálido

        // 4. Botón de Cerrar (X)
        GameObject closeBtnGO = new GameObject("Button_Cerrar", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnGO.transform.SetParent(panelRoot.transform, false);
        RectTransform closeRect = closeBtnGO.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-45, -45);
        closeRect.sizeDelta = new Vector2(40, 40);

        Image closeImage = closeBtnGO.GetComponent<Image>();
        Sprite closeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/UI_Button_Close.png");
        if (closeSprite != null) closeImage.sprite = closeSprite;

        // 5. Área de Contenido interior (para texto de historia, controles o sliders)
        GameObject contentGO = new GameObject("Area_Contenido", typeof(RectTransform));
        contentGO.transform.SetParent(panelRoot.transform, false);
        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.12f, 0.12f);
        contentRect.anchorMax = new Vector2(0.88f, 0.85f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        Selection.activeGameObject = panelRoot;
        Undo.RegisterCreatedObjectUndo(panelRoot, "Create Styled Panel");
        Debug.Log("[Paws&Seeds] Panel estilizado creado exitosamente.");
    }

    [MenuItem("Tools/Paws & Seeds/Crear Boton de Madera", false, 11)]
    public static void CreateWoodButton()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        Transform parent = Selection.activeTransform != null ? Selection.activeTransform : (canvas != null ? canvas.transform : null);

        GameObject btnGO = new GameObject("Button_Madera", typeof(RectTransform), typeof(Image), typeof(Button));
        if (parent != null) btnGO.transform.SetParent(parent, false);

        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 55);

        Image img = btnGO.GetComponent<Image>();
        Sprite normalSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/UI_Button_Normal.png");
        Sprite hoverSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/UI_Button_Hover.png");
        Sprite pressedSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/UI_Button_Pressed.png");

        if (normalSpr != null) img.sprite = normalSpr;
        img.type = Image.Type.Sliced;

        Button btn = btnGO.GetComponent<Button>();
        btn.transition = Selectable.Transition.SpriteSwap;
        SpriteState state = new SpriteState();
        state.highlightedSprite = hoverSpr;
        state.pressedSprite = pressedSpr;
        state.selectedSprite = hoverSpr;
        btn.spriteState = state;

        // Texto del botón
        GameObject textGO = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "BOTON";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 20;
        tmp.color = new Color(0.95f, 0.90f, 0.80f); // Crema / pergamino

        Selection.activeGameObject = btnGO;
        Undo.RegisterCreatedObjectUndo(btnGO, "Create Wood Button");
        Debug.Log("[Paws&Seeds] Botón de madera creado exitosamente.");
    }

    [MenuItem("Tools/Paws & Seeds/Reparar Menu y Botones Automaticamente", false, 1)]
    public static void RepararMenuAutomaticamente()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Paws&Seeds] No se encontró ningún Canvas en la escena.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Reparar Menu UI");

        // 1. Corregir Canvas Scaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 2. Corregir BackgroundImage (Stretch completo)
        Transform bgTrans = canvas.transform.Find("BackgroundImage");
        if (bgTrans != null)
        {
            RectTransform bgRect = bgTrans.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bgRect.localScale = Vector3.one;

            Image bgImg = bgTrans.GetComponent<Image>();
            if (bgImg != null) bgImg.preserveAspect = false;
        }

        // 3. Corregir MenuButtonsPanel
        Transform menuPanelTrans = canvas.transform.Find("MenuButtonsPanel");
        if (menuPanelTrans == null)
        {
            // Podría estar dentro de PressE_Button accidentalmente
            Transform pressE = canvas.transform.Find("PressE_Button");
            if (pressE != null) menuPanelTrans = pressE.Find("MenuButtonsPanel");
        }

        if (menuPanelTrans != null)
        {
            menuPanelTrans.SetParent(canvas.transform, true);
            RectTransform mpRect = menuPanelTrans.GetComponent<RectTransform>();
            mpRect.anchorMin = new Vector2(0.5f, 0.5f);
            mpRect.anchorMax = new Vector2(0.5f, 0.5f);
            mpRect.pivot = new Vector2(0.5f, 0.5f);
            mpRect.anchoredPosition = Vector2.zero;
            mpRect.sizeDelta = new Vector2(1050, 750);
            mpRect.localScale = Vector3.one;

            Image mpImg = menuPanelTrans.GetComponent<Image>();
            if (mpImg == null) mpImg = menuPanelTrans.gameObject.AddComponent<Image>();
            Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/UI_Panel_Parchment.png");
            if (panelSprite != null) mpImg.sprite = panelSprite;
            mpImg.preserveAspect = true;

            // Si MenuButtonsPanel tiene VerticalLayoutGroup, quitarlo porque debe ir en Contenedor_Botones
            VerticalLayoutGroup oldLayout = menuPanelTrans.GetComponent<VerticalLayoutGroup>();
            if (oldLayout != null) Object.DestroyImmediate(oldLayout);

            // 4. Corregir o crear Contenedor_Botones
            Transform contTrans = menuPanelTrans.Find("Contenedor_Botones");
            if (contTrans == null)
            {
                GameObject contGO = new GameObject("Contenedor_Botones", typeof(RectTransform));
                contTrans = contGO.transform;
                contTrans.SetParent(menuPanelTrans, false);
            }

            RectTransform contRect = contTrans.GetComponent<RectTransform>();
            contRect.anchorMin = Vector2.zero;
            contRect.anchorMax = Vector2.one;
            contRect.offsetMin = new Vector2(120, 80);   // Left, Bottom
            contRect.offsetMax = new Vector2(-120, -160); // Right, Top
            contRect.localScale = Vector3.one;
            contRect.localRotation = Quaternion.identity;

            VerticalLayoutGroup vlg = contTrans.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = contTrans.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // 5. Mover y corregir los botones dentro de Contenedor_Botones
            string[] buttonNames = new string[] { "JUGAR", "HISTORIA", "CONTROLERS", "CONTROLADORES", "VOLUMEN", "SALIR" };
            foreach (string bName in buttonNames)
            {
                Transform btnTrans = canvas.transform.Find(bName);
                if (btnTrans == null) btnTrans = menuPanelTrans.Find(bName);
                if (btnTrans == null) btnTrans = contTrans.Find(bName);

                if (btnTrans != null)
                {
                    btnTrans.SetParent(contTrans, false);
                    RectTransform bRect = btnTrans.GetComponent<RectTransform>();
                    bRect.localScale = Vector3.one;
                    bRect.localRotation = Quaternion.identity;
                    bRect.sizeDelta = new Vector2(300, 58);
                    bRect.anchoredPosition = Vector2.zero;
                }
            }
        }

        // 6. Corregir y Ocultar Paneles Secundarios
        string[] panelNames = new string[] { "History_Panel", "Vol_Panel", "Controls_Panel" };
        foreach (string pName in panelNames)
        {
            Transform pTrans = canvas.transform.Find(pName);
            if (pTrans == null && menuPanelTrans != null) pTrans = menuPanelTrans.Find(pName);

            if (pTrans != null)
            {
                pTrans.SetParent(canvas.transform, true);
                pTrans.SetAsLastSibling(); // Al final para dibujarse al frente cuando se abran
                RectTransform pRect = pTrans.GetComponent<RectTransform>();
                pRect.anchorMin = new Vector2(0.5f, 0.5f);
                pRect.anchorMax = new Vector2(0.5f, 0.5f);
                pRect.pivot = new Vector2(0.5f, 0.5f);
                pRect.anchoredPosition = Vector2.zero;
                pRect.sizeDelta = new Vector2(850, 600);
                pRect.localScale = Vector3.one;

                // Desactivar para que arranquen cerrados
                pTrans.gameObject.SetActive(false);
            }
        }

        // 7. Conectar eventos en MainMenuManager y Botones
        MainMenuManager mgr = Object.FindAnyObjectByType<MainMenuManager>();
        if (mgr != null)
        {
            SerializedObject so = new SerializedObject(mgr);

            Transform pressE = canvas.transform.Find("PressE_Button");
            if (pressE != null) so.FindProperty("pressEPrompt").objectReferenceValue = pressE.gameObject;
            if (menuPanelTrans != null) so.FindProperty("menuButtonsPanel").objectReferenceValue = menuPanelTrans.gameObject;

            Transform hp = canvas.transform.Find("History_Panel");
            if (hp != null) so.FindProperty("storyPanel").objectReferenceValue = hp.gameObject;

            Transform cp = canvas.transform.Find("Controls_Panel");
            if (cp != null) so.FindProperty("controlsPanel").objectReferenceValue = cp.gameObject;

            Transform vp = canvas.transform.Find("Vol_Panel");
            if (vp != null) so.FindProperty("settingsPanel").objectReferenceValue = vp.gameObject;

            so.ApplyModifiedProperties();

            // Vincular OnClick en los botones automáticamente
            LinkButton(canvas, "JUGAR", mgr.gameObject, "PlayGame");
            LinkButton(canvas, "HISTORIA", mgr.gameObject, "OpenStory");
            LinkButton(canvas, "CONTROLERS", mgr.gameObject, "OpenControls");
            LinkButton(canvas, "CONTROLADORES", mgr.gameObject, "OpenControls");
            LinkButton(canvas, "VOLUMEN", mgr.gameObject, "OpenSettings");
            LinkButton(canvas, "SALIR", mgr.gameObject, "QuitGame");
            if (pressE != null) LinkButton(canvas, "PressE_Button", mgr.gameObject, "OnPressEStart");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[Paws&Seeds] ¡Menú, escalas, layout y conexiones reparados exitosamente!");
    }

    private static void LinkButton(Canvas canvas, string buttonName, GameObject target, string methodName)
    {
        Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button b in allButtons)
        {
            if (b.gameObject.name == buttonName)
            {
                // Limpiar eventos persistentes previos para evitar duplicados
                while (b.onClick.GetPersistentEventCount() > 0)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(b.onClick, 0);
                }

                System.Reflection.MethodInfo method = typeof(MainMenuManager).GetMethod(methodName);
                if (method != null)
                {
                    UnityEngine.Events.UnityAction action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target.GetComponent<MainMenuManager>(), method);
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick, action);
                    EditorUtility.SetDirty(b);
                }
                break;
            }
        }
    }
}

