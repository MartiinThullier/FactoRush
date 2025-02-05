﻿using System;
using System.Collections.Generic;
using GameContent.CraftResources;
using GameContent.Entities.GridEntities;
using GameContent.GridManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameContent.Entities.OnFieldEntities
{
    public class ConveyorGroup : DynamicBuildingList
    {
        #region properties
        
        public sbyte ConveyorGroupId { get; set; }
        
        public Tile FromStaticTile { get; set; }
        
        public Tile ToStaticTile { get; set; }

        #endregion

        #region constructor

        public ConveyorGroup(sbyte id, params DynamicBuilding[] dl) : base(dl)
        {
            _tempPath = new List<DynamicBuilding>();
            _conveyedResources = new List<BaseResource>();
            ConveyorGroupId = id;
        }

        #endregion

        #region methodes

        public void Init()
        {
            // Ca marche mais j'ai oublié ou est le code qui fait que ca marche
            if ((this[0].TileRef is SideStaticBuildingTile && this[Count - 1].TileRef is MineTile) ||
                (this[0].TileRef is WeaponTargetTile or TransTargetTile && this[Count - 1].TileRef is SideStaticBuildingTile))
                DynamicBuildings.Reverse();

            FromStaticTile = this[0].TileRef;
            ToStaticTile = this[Count - 1].TileRef;
            
            FromStaticTile.AddConveyorGroup(this);
            ToStaticTile.AddConveyorGroup(this);
            FromStaticTile.MarkActive(true);
            ToStaticTile.MarkActive(true);
            
            if (!CheckValidity() || !CheckPathJump())
                return;
            
            GraphInit();
            TrySetRefineryRef();
        }

        public void InitFromDynamic(sbyte index)
        {
            var f = GridManager.Manager.ConveyorGroups[index][0];
            
            if (!(f.TileRef is MineTile && 
                  this[Count - 1].TileRef is SideStaticBuildingTile) &&
                !(f.TileRef is SideStaticBuildingTile &&
                  this[Count - 1].TileRef is TransTargetTile or WeaponTargetTile))
            {
                this[0].RemoveConveyorGroupId(ConveyorGroupId);
                
                for (var j = 1; j < Count; j++)
                {
                    var b = this[j];
                    b.TileRef.CurrentBuildingRef = null;
                    Object.Destroy(b.gameObject);
                }

                GridManager.Manager.ConveyorGroups.Remove(ConveyorGroupId);
                foreach (var d in f)
                {
                    GridManager.Manager.ConveyorGroups[d].UpdateGroup();
                }
                return;
            }
            
            var c = GridManager.Manager.ConveyorGroups[index];
            
            foreach (var t in c)
            {
                if (t.TileRef.Index != this[0].TileRef.Index)
                {
                    _tempPath.Add(t);
                    t.AddConveyorGroupId(ConveyorGroupId);
                    t.SetDebugId(); //ConveyorGroupId
                }

                else
                    break;
            }
            
            GraphInit();
            
            DynamicBuildings.InsertRange(0, _tempPath);
            
            FromStaticTile = this[0].TileRef;
            ToStaticTile = this[Count - 1].TileRef;
            
            FromStaticTile.AddConveyorGroup(this);
            ToStaticTile.AddConveyorGroup(this);
            FromStaticTile.MarkActive(true);
            ToStaticTile.MarkActive(true);
            
            TrySetRefineryRef();
            
            _tempPath.Clear();
        }
        
        private void GraphInit()
        {
            for (var i = 0; i < Count; i++)
            {
                if (i >= Count - 1)
                    continue;
                
                var r = DynamicBuilding.GetRotation(this[i + 1].TileRef.Index - this[i].TileRef.Index);
                this[i].SetGraph(this[i].Position, Quaternion.Euler(0, r, 0));
            }

            if (this[Count - 1].TileRef is not SideStaticBuildingTile t)
                return;

            var s = GridManager.Manager.StaticGroups[t.StaticGroup];
            var r2 = DynamicBuilding.GetRotation(s[2].Index - this[Count - 1].TileRef.Index);
            this[Count - 1].SetGraph(this[Count - 1].Position, Quaternion.Euler(0, r2, 0));
        }
        
        private bool CheckPathJump()
        {
            var j = 0;
            
            for (var i = 0; i < Count - 1; i++)
            {
                if (Vector2Int.Distance(this[i].TileRef.Index, this[i + 1].TileRef.Index) < 1.1f)
                    j++;
            }
            
            if (j >= Count - 1)
                return true;
            
            foreach (var b in this)
            {
                b.TileRef.CurrentBuildingRef = null;
                Object.Destroy(b.gameObject);
            }
            GridManager.Manager.ConveyorGroups.Remove(ConveyorGroupId);
            UnsetSelf();
            return false;
        }
        
        private bool CheckValidity()
        {
            if (FromStaticTile is not DynamicBuildingTile && ToStaticTile is not DynamicBuildingTile && Count > 1)
                return true;

            foreach (var b in this)
            {
                b.TileRef.CurrentBuildingRef = null;
                Object.Destroy(b.gameObject);
            }
            GridManager.Manager.ConveyorGroups.Remove(ConveyorGroupId);
            UnsetSelf();
            return false;
        }

        private void TrySetRefineryRef()
        {
            if (FromStaticTile is not StaticBuildingTile s)
                return;
            
            var c = GridManager.Manager.StaticGroups[s.StaticGroup].CenterRef;
            c.AddConveyorGroup(this);
        }
        
        public void AddResource(BaseResource resource) => _conveyedResources.Add(resource);
        
        public void RemoveResource(BaseResource resource) => _conveyedResources.Remove(resource);
        
        public void DestroyGroup()
        {
            FromStaticTile.MarkActive(false);
            ToStaticTile.MarkActive(false);

            if (_conveyedResources.Count > 0)
            {
                foreach (var r in _conveyedResources)
                {
                    Object.Destroy(r.gameObject);
                }
                _conveyedResources.Clear();
            }
            
            FromStaticTile.RemoveConveyorGroup(this);
            ToStaticTile.RemoveConveyorGroup(this);

            if (FromStaticTile.GroupRef.Count > 0)
                FromStaticTile.MarkActive(true);
        }

        public override void UpdateGroup()
        {
            foreach (var d in DynamicBuildings)
            {
                d.UpdateBuild();
            }
        }
        
        public void MarkActive(bool active)
        {
            ToStaticTile.MarkActive(active);
            FromStaticTile.MarkActive(active);
        }
        
        private void UnsetSelf()
        {
            FromStaticTile.MarkActive(false);
            ToStaticTile.MarkActive(false);
            FromStaticTile.RemoveConveyorGroup(this);
            ToStaticTile.RemoveConveyorGroup(this);
        }

        #endregion

        #region fields

        private readonly List<DynamicBuilding> _tempPath;
        
        private readonly List<BaseResource> _conveyedResources;

        //C'est pas fou en fait
        private static readonly Comparison<DynamicBuilding> TypeComparer = (a, b) => (int)Mathf.Sign((byte)a.TileRef.Type - (byte)b.TileRef.Type);

        #endregion
    }
}