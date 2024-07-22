using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Patterns.ServiceLocator;
using UnityEngine;

namespace Timba.SacredTails.Photoboot
{
    /// <summary>
    ///     Service that allows take pictures of Shinsei for UI
    /// </summary>
    public interface IIconGeneration : IService
    {
        public void GenerateShinseiIcons(List<Shinsei> shinseiParty, Action callback = null);
    }
}