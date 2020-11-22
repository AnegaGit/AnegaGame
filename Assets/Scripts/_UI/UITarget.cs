/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public partial class UITarget : MonoBehaviour
{
    public GameObject panel;
    public Slider healthSlider;
    public Image healthSliderBar;
    public Button targetButton;
    public Text nameText;
    public Transform buffsPanel;
    public UIBuffSlot buffSlotPrefab;
    public Button tradeButton;
    public Button guildInviteButton;
    public Button partyInviteButton;
    public Button attackButton;
    public UICharacterExamination characterExamination;
    public UINpcTrading uiNpcTrading;

    private Entity target;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            // show nextTarget > target
            // => feels best in situations where we select another target while
            //    casting a spell on the existing target.
            // => '.target' doesn't change while casting, but the UI gives the
            //    illusion that we already targeted something else
            // => this is also great for spells that change the target while casting,
            //    e.g. a buff that is cast on 'self' even though we target an 'npc.
            //    this way the player doesn't see the target switching.
            // => this is how most MMORPGs do it too.
            target = player.nextTarget ?? player.target;
            if (target != null && (target != player || player.isGM))
            {
                float distance = Utils.ClosestDistance(player.collider, target.collider);
                if (distance <= player.distanceDetectionPerson)
                {
                    // name and health
                    panel.SetActive(true);
                    switch (player.abilities.diagnosis)
                    {
                        case 0:
                            healthSlider.value = 1;
                            if (target.health == 0)
                                healthSliderBar.color = PlayerPreferences.healthColorDeath;
                            else
                                healthSliderBar.color = PlayerPreferences.healthColorUnharmed;
                            break;
                        case 2:
                            healthSlider.value = ((int)(target.HealthPercent() * 13)) / 13.0f;
                            healthSliderBar.color = HealthColor(target.HealthPercent());
                            break;
                        case 3:
                            healthSlider.value = target.HealthPercent();
                            healthSliderBar.color = HealthColor(target.HealthPercent());
                            break;
                        default:
                            healthSlider.value = 1;
                            healthSliderBar.color = HealthColor(target.HealthPercent());
                            break;
                    }

                    // name button
                    if (target is Player)
                    {
                        Player targetPlayer = (Player)target;
                        string text = nameText.text = player.KnownName(targetPlayer.id);
                        if (player.isGM && GameMaster.knowNames(player.gmState))
                        {
                            if (text == GlobalVar.nameNotKnown)
                            {
                                text = targetPlayer.displayName;
                            }
                        }
                        if (targetPlayer.id == player.id)
                        {
                            text = "Yourself";
                        }
                        nameText.text = text;
                    }
                    else
                        nameText.text = target.displayedName;

                    // target buffs
                    UIUtils.BalancePrefabs(buffSlotPrefab.gameObject, target.buffs.Count, buffsPanel);
                    for (int i = 0; i < target.buffs.Count; ++i)
                    {
                        UIBuffSlot slot = buffsPanel.GetChild(i).GetComponent<UIBuffSlot>();

                        // refresh
                        slot.image.color = Color.white;
                        slot.image.sprite = target.buffs[i].image;
                        slot.tooltip.text = target.buffs[i].ToolTip();
                        slot.slider.maxValue = target.buffs[i].buffTime;
                        slot.slider.value = target.buffs[i].BuffTimeRemaining();
                    }

                    // trade button
                    if (target is Player)
                    {
                        tradeButton.gameObject.SetActive(true);
                        tradeButton.interactable = distance <= player.interactionRange;
                    }
                    else if (target is Npc)
                    {
                        tradeButton.gameObject.SetActive(true);
                        tradeButton.interactable = distance <= player.interactionRange;
                    }
                    else
                    {
                        tradeButton.gameObject.SetActive(false);
                    }

                    // guild invite button
                    if (target is Player && player.InGuild())
                    {
                        guildInviteButton.gameObject.SetActive(true);
                        guildInviteButton.interactable = !((Player)target).InGuild() &&
                                                         player.guild.CanInvite(player.name, target.name) &&
                                                         NetworkTime.time >= player.nextRiskyActionTime &&
                                                         distance <= player.interactionRange;
                        guildInviteButton.onClick.SetListener(() =>
                        {
                            player.CmdGuildInviteTarget();
                        });
                    }
                    else guildInviteButton.gameObject.SetActive(false);

                    // party invite button
                    if (target is Player)
                    {
                        partyInviteButton.gameObject.SetActive(true);
                        partyInviteButton.interactable = (!player.InParty() || player.party.CanInvite(player.name)) &&
                                                         !((Player)target).InParty() &&
                                                         NetworkTime.time >= player.nextRiskyActionTime &&
                                                         distance <= player.interactionRange;
                        partyInviteButton.onClick.SetListener(() =>
                        {
                            player.CmdPartyInvite(target.name);
                        });
                    }
                    else partyInviteButton.gameObject.SetActive(false);

                    if ((target is Player || target is Fighter) && player.WeaponEquipped())
                    {
                        attackButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        attackButton.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // target out of detection range, delete it
                    player.CmdClearTarget();
                    panel.SetActive(false);
                }
            }
            else panel.SetActive(false);

        }
        else panel.SetActive(false);
    }
    private Color HealthColor(float healthPercent)
    {
        if (healthPercent > GlobalVar.healthLimitUnharmed)
            return PlayerPreferences.healthColorUnharmed;
        else if (healthPercent > GlobalVar.healthLimitSlightlyWounded)
            return PlayerPreferences.healthColorSlightlyWounded;
        else if (healthPercent > GlobalVar.healthLimitWounded)
            return PlayerPreferences.healthColorWounded;
        else if (healthPercent > GlobalVar.healthLimitBadlyWounded)
            return PlayerPreferences.healthColorBadlyWounded;
        else if (healthPercent > 0)
            return PlayerPreferences.healthColorNearDeath;
        else
            return PlayerPreferences.healthColorDeath;
    }
    public void OnClickCharacterExamination()
    {
        if (target is Player)
            characterExamination.InitializePlayer((Player)target);
        else
            characterExamination.InitializeEntity(target);
    }
    public void OnClickTrade()
    {
        if (target is Npc)
        {
            uiNpcTrading.StartStopTrading();
        }
        else
            Debug.Log(">>> no NPC");
    }
    public void OnClickAttack()
    {
        if (target is Fighter)
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                player.TryAttackStandardFight();
            }
        }
        else
        {
            LogFile.WriteLog(LogFile.LogLevel.Warning, string.Format("Try to attack {0} from target UI.", target.name));
        }
    }
}
