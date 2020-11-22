/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

public class WardrobeDisplayItem : MonoBehaviour
{
    DynamicCharacterAvatar UMAAvatar;
    public UMATextRecipe hideBody;
    public UMATextRecipe[] applyGarment;

    void Awake()
    {
        UMAAvatar = GetComponent<DynamicCharacterAvatar>();
        UMAAvatar.CharacterCreated.AddListener(UMACharacterCreated);
    }
    void UMACharacterCreated(UMAData arg0)
    {
        UMAAvatar.CharacterCreated.RemoveListener(UMACharacterCreated);
        Invoke("UMACharacterCreatedFinished", 0.1f);
    }
    void UMACharacterCreatedFinished()
    {
        //Initialize
        UMAAvatar.SetSlot(hideBody.wardrobeSlot, hideBody.name);

        foreach (UMATextRecipe recipe in applyGarment)
        {
            UMAAvatar.SetSlot(recipe.wardrobeSlot, recipe.name);
        }
        UMAAvatar.BuildCharacter();
    }
}
