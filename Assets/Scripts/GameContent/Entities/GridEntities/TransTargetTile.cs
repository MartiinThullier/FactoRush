﻿namespace GameContent.Entities.GridEntities
{
    /// <summary>
    /// Even id key
    /// </summary>
    public sealed class TransTargetTile : Tile
    {
        #region properties

        public override bool IsBlocked => true;

        public override bool IsSelected
        {
            get => false;
            set {}
        }

        #endregion

        #region fields

        //TODO queue

        #endregion
    }
}