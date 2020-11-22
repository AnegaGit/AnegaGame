/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Simple character selection list. The charcter prefabs are known, so we could
// easily show 3D models, stats, etc. too .
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public partial class UICharacterSelection : MonoBehaviour
{
    public UICharacterCreation uiCharacterCreation;
    public NetworkManagerMMO manager; // singleton is null until update
    public GameObject panel;
    public Transform cameraLocation;
    public float cameraFieldOfView;
    public Button startButton;
    public Button deleteButton;
    public Button createButton;
    public Button quitButton;

    private bool isInitialized = false;
    void Update()
    {
        // show while in lobby and while not creating a character
        if (manager.state == NetworkState.Lobby && !uiCharacterCreation.IsVisible())
        {
            panel.SetActive(true);
            if (!isInitialized)
            {
                isInitialized = true;
                PanelLoading panelLoading = GetComponent<PanelLoading>();
                panelLoading.Activate(0.3f, 1.5f);
                // setup camera
                Camera.main.transform.position = cameraLocation.position;
                Camera.main.transform.rotation = cameraLocation.rotation;
                Camera.main.fieldOfView = cameraFieldOfView;
            }
            // characters available message received already?
            if (manager.charactersAvailableMsg != null)
            {
                CharactersAvailableMsg.CharacterPreview[] characters = manager.charactersAvailableMsg.characters;
                // start button: calls AddPLayer which calls OnServerAddPlayer
                // -> button sends a request to the server
                // -> if we press button again while request hasn't finished
                //    then we will get the error:
                //    'ClientScene::AddPlayer: playerControllerId of 0 already in use.'
                //    which will happen sometimes at low-fps or high-latency
                // -> internally ClientScene.AddPlayer adds to localPlayers
                //    immediately, so let's check that first
                startButton.gameObject.SetActive(manager.selection != -1);
                startButton.onClick.SetListener(() => {
                    // start Loading sequence
                    Universal.LoadingPanel.Activate(GlobalVar.loadingPanelBlackSeconds, GlobalVar.loadingPanelFadeSeconds);
                    // add player
                    CharacterSelectMsg message = new CharacterSelectMsg{index=manager.selection};
                    ClientScene.AddPlayer(manager.client.connection, message);
                    // clear character selection previews
                    manager.ClearPreviews();

                    // make sure we can't select twice and call AddPlayer twice
                    panel.SetActive(false);
                    // remove scene from view
                    GameObject characterSelectionArea = GameObject.Find("Areas/CharacterSelection");
                    characterSelectionArea.SetActive(false);                });
                // delete button
                deleteButton.gameObject.SetActive(manager.selection != -1);
                deleteButton.onClick.SetListener(() => {
                    CharacterDeleteMsg message = new CharacterDeleteMsg{index=manager.selection};
                    manager.client.Send(CharacterDeleteMsg.MsgId, message);
                });
                // create button
                createButton.interactable = characters.Length < manager.characterLimit;
                createButton.onClick.SetListener(() => {
                    panel.SetActive(false);
                    uiCharacterCreation.Show();
                });
                // quit button
                quitButton.onClick.SetListener(() => { NetworkManagerMMO.Quit(); });
            }
        }
        else
        {
            panel.SetActive(false);
            isInitialized = false;
        }

    }
}
