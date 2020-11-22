/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using UMA;
using UMA.CharacterSystem;

public class ElementSlot : NetworkBehaviour
{
    [Header("Base Properties")]
    [SyncVar] public string specialName = "";
    public string cannotRead = "";
    [SyncVar] public Item _item;
    [SyncVar] public bool applyToGround = true;
    public GameObject model;
    [SyncVar] public int amount = 1;
    public bool isStatic = false;
    public GameObject amountOverlay;
    public TextMesh amountText;
 //   [ShowTimeFromSeconds]
    public int decayAfter = int.MaxValue;
    public LayerMask groundLayers;


    // logic
    private UsableItem usableItem; // since used often
    // client side use only, need for open container, portal etc, each client different
    [HideInInspector] public bool isInUse = false;
    [HideInInspector] public int lastData1 = -1;

    [Header("Tooltip")]
    [TextArea(1, 30)] public string toolTip = "";

    // instantiated tooltip
    Transform canvas;
    GameObject current;

    //UMA recipe
    private DynamicCharacterAvatar UMAAvatar;
    private bool isUMAItem = false;

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").transform;
    }

    private void Start()
    {
        usableItem = (UsableItem)_item.data;
        if (isClient)
        {
            if (!isStatic)
            {
                InitializeModel();
                usableItem.OnUseAction(this, _item.data1, _item.data2, _item.data3);
            }
            if (applyToGround)
                InitializeToGround();
            Universal.ChangeLayers(gameObject, GlobalVar.layerElement);

        }
        if (isServer)
        {
            usableItem.Initialize(this);
        }
    }


    private void Update()
    {
        // no need for graphic on server
        if (isClient)
        {
            usableItem.UpdateClient(this);
        }
        if (isServer)
        {
            usableItem.UpdateServer(this);
        }
    }
    public Item item
    {
        get { return _item; }
        set { _item = value; }
    }
    public void InitializeModel()
    {
        if (usableItem)
        {
            // first remove older models if exists
            if (model)
                Destroy(model);
            if (usableItem.modelPrefab)
            {
                model = Instantiate(usableItem.modelPrefab, this.transform);
                model.transform.SetParent(this.transform, true);
            }
        }
    }

    public float interactionRange
    {
        get
        {
            float bestValue = -1;
            if (usableItem)
            {
                return usableItem.interactionRange;
            }
            return bestValue;
        }
    }
    public bool pickable
    {
        get
        {
            if (usableItem)
            {
                return usableItem.CanPicked(this);
            }
            return false;
        }
    }
    //can this player use this element
    public bool CanUse(Player player)
    {
        return ((UsableItem)_item.data).CanUse(player, this);
    }

    public void InitializeToGround()
    {
        //verify whether the item is a UMA wardrobe and assign default recipe
        if (_item.data is ClothingItem)
        {
            ClothingItem ci = (ClothingItem)_item.data;
            if (ci.UMAClothingRecipeMale)
            {
                isUMAItem = true;
                UMAAvatar = model.transform.Find("DCA").gameObject.GetComponent<DynamicCharacterAvatar>();
                UMAAvatar.CharacterCreated.AddListener(UMACharacterCreated);
            }
        }

        // move this object out of layer, layer will be applied in every case later
        Universal.ChangeLayers(gameObject, GlobalVar.layerIgnoreRaycast);
        Vector3 startPos = transform.position;
        // find any other collider
        RaycastHit hit;
        Vector3 rcStartPos = startPos;
        rcStartPos.y = rcStartPos.y + 2f;
        var ray = new Ray(rcStartPos, Vector3.down);
        if (Physics.Raycast(ray, out hit, 5f, GlobalVar.layerMaskGround))
        {
            startPos = hit.point;
            // take slopes into consideration
            // transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation; // adjust for slopes
        }

        Vector3 boundPosition = new Vector3();
        float yMin = 100000f;
        int i = 0;
        foreach (Collider col in model.GetComponentsInChildren<Collider>())
        {
            i++;
            //find lowest point in collider
            if (col.bounds.min.y < yMin)
                yMin = col.bounds.min.y;
            // summarize for center
            if (i == 1)
                boundPosition = col.bounds.center;
            else
                boundPosition += col.bounds.center;
        }
        if (i == 0)
        {
            LogFile.WriteLog(LogFile.LogLevel.Error, string.Format("Model {0} has no Collider attached. The related item {1} has a configuration error.", model.name, item.name));
            return;
        }

        boundPosition = boundPosition / i;
        Vector3 newPos = startPos + transform.position;
        newPos.x -= boundPosition.x;
        newPos.y -= yMin;
        newPos.z -= boundPosition.z;

        transform.position = newPos;

        if (amount == 1)
            amountOverlay.SetActive(false);
        else
        {
            amountOverlay.SetActive(true);
            amountOverlay.transform.position = new Vector3(startPos.x, startPos.y + boundPosition.y - yMin + 0.175f, startPos.z);
            amountText.text = amount.ToString();
        }
        if (isUMAItem)
        {
            // make invisible until UMA has been initialized
            transform.localScale = new Vector3(0, 0, 0);
        }
    }

    void UMACharacterCreated(UMAData arg0)
    {
        UMAAvatar.CharacterCreated.RemoveListener(UMACharacterCreated);
        Invoke("UMAApplyWardrobe", 0.1f);
        //  delay required for UMA finishing processing
    }
    void UMAApplyWardrobe()
    {
        ClothingItem ci = (ClothingItem)_item.data;

        UMAAvatar.SetSlot(ci.category, ci.UMAClothingRecipeMale.name);
        UMAAvatar.SetSlot("Body", Universal.RecipeHideBody.name);
        UMAAvatar.BuildCharacter();
        transform.localScale = new Vector3(1, 1, 1);
    }

    public string displayName
    {
        get
        {
            string tmp = specialName;
            if (specialName.Length == 0)
                tmp = _item.name;
            return tmp;
        }
        set
        {
            if (value == _item.name)
                specialName = "";
            else
                specialName = value;
        }
    }
    public string readDisplayName()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            if (player.abilities.readAndWrite != Abilities.Nav)
                return displayName;
        }
        return cannotRead;
    }

    public void SetItemData(int data1, int data2 = 0, int data3 = 0)
    {
        _item.data1 = data1;
        _item.data2 = data2;
        _item.data3 = data3;
        SetDirtyBit(GlobalVar.allDirty);
    }

    public void SetItemQuality(int quality)
    {
        _item.quality = quality;
        SetDirtyBit(GlobalVar.allDirty);
    }

    public void SetItemDurability(int durability)
    {
        _item.durability = durability;
        SetDirtyBit(GlobalVar.allDirty);
    }

    // using the Element
    //   ElementSlot/Player            useable item
    //
    //      Start-----------------> Initialize 
    //                                              
    //       UseBy                                         from client
    //        ||                                    
    //    CmdUseElement                             
    //         ------------------->     Use   -->---       First or single use on server
    //        ||              ----<------          |
    //    RpcUsedElement      |                    |
    //         ---------------)--->    OnUsed      |       First or single use on client
    //                        |                    |
    //    UseOverTime <-------o                    |                    
    //        ||              |                    |
    //   UseOverTimeEnd ------)------>   InUse     |       Repeated use on server
    //                        |            |       |
    //                        ---<---------o       |
    //                                     |       |
    //     RPCUseAction <------------------o--------
    //         |
    //         ---------------------->  OnUseAction        Action on client / called on Start too
    //
    //                                   UseEnd            Client end of usage

    //[Server]
    public void UseOverTime(float waitTime)
    {
        Invoke("UseOverTimeEnd", waitTime);
    }
    //[Server]
    public void UseOverTimeEnd()
    {
        usableItem.InUse(this);
    }

    [ClientRpc]
    public void RpcUseAction(int data1, int data2, int data3)
    {
        // make sure model is initialized
        if (model || usableItem)
        {
            usableItem.OnUseAction(this, data1, data2, data3);
        }
    }

    // Tooltip part
    void CreateToolTip()
    {
        if (!current)
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance < player.distanceDetectionPerson)
                {
                    // instantiate
                    current = Instantiate(Universal.ToolTipPrefab, Input.mousePosition, Quaternion.identity);

                    // put to foreground
                    current.transform.SetParent(canvas, true); // canvas
                    current.transform.SetAsLastSibling(); // last one means foreground
                    current.GetComponentInChildren<Text>().text = ToolTip();
                }
            }
        }
    }
    bool isTooltip = false;
    void DisplayToolTip(float delay)
    {
        isTooltip = true;
        Invoke("CreateToolTip", delay);
    }
    void DestroyToolTip()
    {
        isTooltip = false;
        // stop any running attempts to show it
        CancelInvoke("CreateToolTip");
        // destroy it
        Destroy(current);
    }
    public void OnMouseOver()
    {
        if (!Utils.IsCursorOverUserInterface() && !isTooltip)
        {
            DisplayToolTip(0.5f);
        }
        if (Utils.IsCursorOverUserInterface() && isTooltip)
        {
            DestroyToolTip();
        }
    }
    public void OnMouseExit()
    {
        DestroyToolTip();
    }
    void OnDisable()
    {
        DestroyToolTip();
    }
    void OnDestroy()
    {
        DestroyToolTip();
    }
    public virtual string ToolTip()
    {
        Player player = Player.localPlayer;
        // we use a StringBuilder so it is easy to modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(item.ToolTip());
        tip.Replace("{SPECIALNAME}", displayName);
        tip.Replace("{INSCRIPTION}", readDisplayName());
        tip.Replace("{AMOUNT}", amount.ToString());
        tip.Replace("{TOTALPRICE}", Money.MoneyText(amount * item.price));
        tip.Replace("{TOTALWEIGHT}", GlobalFunc.WeightText((int)(amount * item.weight * player.divergenceWeight), player.handscale));

        // handling the dynamic part of items here
        if (item.data is LightItem)
        {
            LightItem lightItem = (LightItem)item.data;
            tip.Replace("{LIGHTTIME}", LightItem.RemainingLightTime(item.data1, lightItem.maxLightSeconds));
        }
        // handling the dynamic part of items here
        if (item.data is GatheringSourceItem)
        {
            GatheringLifePhase currentPhase = ((GatheringSourceItem)item.data).lifePhases[item.data1];
            if (currentPhase.content.Length == 0)
            {
                tip.Replace("{RESOURCE}", GlobalVar.gatheringTextNoResource);
                tip.Replace("{TOOL}", GlobalVar.gatheringTextNoTool);
            }
            else
            {
                float skillDifference = player.skills.LevelOfSkill(currentPhase.gatheringSkill) - currentPhase.bestSkillLevel;
                tip.Replace("{RESOURCE}", string.Format(GlobalVar.gatheringTextHasResource, GlobalFunc.ExamineLimitText(skillDifference, GlobalVar.gatheringSkillRange)));
                tip.Replace("{TOOL}", string.Format(GlobalVar.gatheringTextTool, Skills.info[(int)currentPhase.gatheringSkill].defaultTool));
            }
        }
        return tip.ToString();
    }

}
