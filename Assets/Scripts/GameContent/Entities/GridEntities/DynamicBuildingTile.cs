﻿using UnityEngine;

namespace GameContent.Entities.GridEntities
{
    public sealed class DynamicBuildingTile : Tile
    {
        #region properties

        public override bool IsBlocked => CurrentBuildingRef is not null;

        public override bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                preSelectionAvailable.SetActive(value);
            }
        }

        #endregion

        #region fields

        [SerializeField] private GameObject preSelectionAvailable;
        
        [SerializeField] private GameObject preSelectionUnavailable; 
        
        private bool _isSelected;

        #endregion
    }
}