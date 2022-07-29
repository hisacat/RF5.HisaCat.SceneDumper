using UnityEngine;

namespace RF5.HisaCat.SceneDumper
{
    public static class DefinedComponentTypes
    {
        public abstract class DefinedComponentType
        {
            public abstract void Convert(Component component);
        }

        [System.Serializable]
        public class TransformData : DefinedComponentType
        {
            public string Position; //Vector3
            public string LocalPosition; //Vector3
            public string EulerAngles; //Vector3
            public string LocalEulerAngles; //Vector3
            //public string Right; //Vector3
            //public string Up; //Vector3
            //public string Forward; //Vector3
            //public string LocalRotation; //Quaternion
            //public string Rotation; //Quaternion
            public string LocalScale; //Vector3
            //public bool HasChanged;
            //public int HierarchyCapacity;
            //public RotationOrder RotationOrder;

            public static bool IsTypeOf(Il2CppSystem.Type type)
            {
                return type == UnhollowerRuntimeLib.Il2CppType.Of<Transform>();
            }
            public override void Convert(Component component)
            {
                var origin = component.Cast<Transform>();
                if (origin == null)
                    throw new System.InvalidCastException("component InvalidCast Transform");

                this.Position = origin.position.ToString();
                this.LocalPosition = origin.localPosition.ToString();
                this.EulerAngles = origin.eulerAngles.ToString();
                this.LocalEulerAngles = origin.localEulerAngles.ToString();
                //this.Right = origin.right.ToString();
                //this.Up = origin.up.ToString();
                //this.Forward = origin.forward.ToString();
                //this.LocalRotation = origin.localRotation.ToString();
                this.LocalScale = origin.localScale.ToString();
                //this.HasChanged = origin.hasChanged;
                //this.HierarchyCapacity = origin.hierarchyCapacity;
                //this.RotationOrder = origin.rotationOrder;
            }
        }
        [System.Serializable]
        public class RectTransformData : DefinedComponentType
        {
            //public string OffsetMin; //Vector2
            //public string AnchoredPosition3D; //Vector3
            public string Pivot; //Vector2
            public string SizeDelta; //Vector2
            public string AnchoredPosition; //Vector2
            //public string AnchorMax; //Vector2
            //public string AnchorMin; //Vector2
            public string Rect; //Rect
            //public DrivenTransformProperties DrivenProperties;
            //public string OffsetMax; //Vector2

            public static bool IsTypeOf(Il2CppSystem.Type type)
            {
                return type == UnhollowerRuntimeLib.Il2CppType.Of<RectTransform>();
            }
            public override void Convert(Component component)
            {
                var origin = component.Cast<RectTransform>();
                if (origin == null)
                    throw new System.InvalidCastException("component InvalidCast RectTransform");

                //this.OffsetMin = origin.offsetMin.ToString();
                //this.AnchoredPosition3D = origin.anchoredPosition3D.ToString();
                this.Pivot = origin.pivot.ToString();
                this.SizeDelta = origin.sizeDelta.ToString();
                this.AnchoredPosition = origin.anchoredPosition.ToString();
                //this.AnchorMax = origin.anchorMax.ToString();
                //this.AnchorMin = origin.anchorMin.ToString();
                //this.AnchorMin = origin.anchorMin.ToString();
                this.Rect = origin.rect.ToString();
                //this.DrivenProperties = origin.drivenProperties;
                //this.OffsetMax = origin.offsetMax.ToString();
            }
        }
    }
}
