/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public partial class UIShortcuts : MonoBehaviour
{
    public GameObject panel;
    public Button inventoryButton;
    public UIInventory inventory;
    public Button equipmentButton;
    public GameObject equipmentPanel;
    public Button handButton;
    public Sprite handImageEmpty;
    public Button spellsButton;
    public GameObject spellsPanel;
    public Button characterInfoButton;
    public GameObject characterInfoPanel;
    public Button questsButton;
    public GameObject questsPanel;
    public Button splitButton;
    public GameObject splitPanel;
    public Text splitText;
    public Button partyButton;
    public GameObject partyPanel;
    public Button cameraButton;
    public Sprite cameraPerson;
    public Sprite cameraWorld;
    public CameraMMO cameraMMO;
    public Button quitButton;
    bool lastRotationFollow = true;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            panel.SetActive(!player.webcamActive);
            if (lastRotationFollow != cameraMMO.rotationFollowPlayer)
            {
                lastRotationFollow = cameraMMO.rotationFollowPlayer;
                if (lastRotationFollow)
                    cameraButton.image.sprite = cameraPerson;
                else
                    cameraButton.image.sprite = cameraWorld;

            }
            splitText.text = player.splitValue.ToString();
            if (player.WeaponEquipped(out WeaponItem weaponItem))
            {
                handButton.image.sprite = weaponItem.image;
            }
            else if (player.GatheringToolEquipped(out GatheringToolItem toolItem))
            {
                handButton.image.sprite = toolItem.image;
            }
            else
            {
                handButton.image.sprite = handImageEmpty;
            }
            inventoryButton.onClick.SetListener(() =>
            {
                int idOfBackpack = player.ContainerIdOfBackpack();
                if (idOfBackpack == -1)
                    player.Inform("You don't have a backpack.");
                else
                    inventory.OpenCloseContainer(idOfBackpack);
            });
            equipmentButton.onClick.SetListener(() =>
            {
                equipmentPanel.SetActive(!equipmentPanel.activeSelf);
            });
            spellsButton.onClick.SetListener(() =>
            {
                spellsPanel.SetActive(!spellsPanel.activeSelf);
            });
            characterInfoButton.onClick.SetListener(() =>
            {
                characterInfoPanel.SetActive(!characterInfoPanel.activeSelf);
            });
            questsButton.onClick.SetListener(() =>
            {
                questsPanel.SetActive(!questsPanel.activeSelf);
            });
            partyButton.onClick.SetListener(() =>
            {
                partyPanel.SetActive(!partyPanel.activeSelf);
            });
            splitButton.onClick.SetListener(() =>
            {
                splitPanel.SetActive(!splitPanel.activeSelf);
            });
            cameraButton.onClick.SetListener(() =>
            {
                cameraMMO.rotationFollowPlayer = !cameraMMO.rotationFollowPlayer;
            });
            quitButton.onClick.SetListener(() =>
            {
                NetworkManagerMMO.Quit();
            });
        }
        else panel.SetActive(false);
    }

    public void ButtonHandClicked()
    {
        Player player = Player.localPlayer;
        if ((player.target is Monster || player.target is Player) && player.WeaponEquipped())
        {
            if (player.CanAttack(player.target))
            {
                int standardSpellId = player.spells.IdOfStandardFighting();
                if (standardSpellId >= 0)
                {
                    if (Vector3.Distance(player.transform.position, player.target.transform.position) > player.spells[standardSpellId].CastRange(player))
                    {
                        // GotoTarget
                        Vector3 targetPos = player.target.collider.ClosestPoint(player.transform.position);
                        player.agent.stoppingDistance = player.spells[standardSpellId].CastRange(player) * GlobalVar.walkInInteractionRange;
                        player.SetIndicatorViaPosition(targetPos);
                        player.agent.destination = targetPos;

                        player.useSpellWhenCloser = standardSpellId;
                    }
                    else

                    {
                        // then try to use that one
                        player.TryUseSpell(standardSpellId);
                    }
                }
            }
        }
        else if (player.GatheringToolEquipped(out GatheringToolItem toolItem))
        {
            // search first right hand
            if (player.inventory.GetEquipment(GlobalVar.equipmentRightHand, out ItemSlot itemSlot))
            {
                if (itemSlot.item.data is GatheringToolItem)
                {
                    player.CmdUseInventoryItem(GlobalVar.containerEquipment, GlobalVar.equipmentRightHand);
                }
            }
            else
            {
                player.CmdUseInventoryItem(GlobalVar.containerEquipment, GlobalVar.equipmentLeftHand);
            }
        }
    }
}
