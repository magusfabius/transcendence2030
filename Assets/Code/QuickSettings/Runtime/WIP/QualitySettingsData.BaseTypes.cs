using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Rendering;
using Attribute = System.Attribute;
using SerializableAttribute = System.SerializableAttribute;
using NonSerializedAttribute = System.NonSerializedAttribute;

public partial class QualitySettingsData
{
    // Probably remove
    public class LaunchParameterAttribute : Attribute {}
    public class QualitySettingParameterAttribute : Attribute {}
    public class GlobalStateParameterAttribute : Attribute {}
    public class RenderPipelineAssetParameterAttribute : Attribute {}
    public class VolumeParameterAttribute : Attribute {}
    public class SceneStateParameterAttribute : Attribute {}
    public class CustomParameterAttribute : Attribute {}

    // // Probably remove
    // public interface IQualitySettingParameter {}
    // public interface IGlobalStateParameter {}
    // public interface IRenderPipelineAssetParameter {}
    // public interface IVolumeParameter {}
    // public interface ISceneStateParameter {}
    // public interface ICustomParameter {}

    public class QualityParameter<T> : VolumeParameter<T>, System.ICloneable
    {
        public object Clone() => MemberwiseClone();
    }

    [Serializable] public class BoolParameter : QualityParameter<bool> {}
    [Serializable] public class IntParameter : QualityParameter<int> {}
    [Serializable] public class FloatParameter : QualityParameter<float> {}
    [Serializable] public class RangeIntParameter : IntParameter { [NonSerialized] public int min, max; }
    [Serializable] public class RangeFloatParameter : FloatParameter { [NonSerialized] public float min, max; }
    [Serializable] public class UnitRangeFloatParameter : RangeFloatParameter { public UnitRangeFloatParameter() { min = 0f; max = 1f; } }
    [Serializable] public class UnitRangeIntParameter : RangeIntParameter { public UnitRangeIntParameter() { min = 0; max = 1; } }
    [Serializable] public class PercentageRangeFloatParameter : RangeFloatParameter { public PercentageRangeFloatParameter() { min = 0f; max = 100f; } }
    [Serializable] public class PercentageRangeIntParameter : RangeIntParameter { public PercentageRangeIntParameter() { min = 0; max = 100; } }
    
    public abstract class QualitySettingsComponent
    {
        VolumeParameter[] m_ParametersCache;
        
        VolumeParameter[] GetParameters()
        {
            if(m_ParametersCache == null)
                m_ParametersCache = GetParameters(this);
            
            return m_ParametersCache;
        }

        public static VolumeParameter[] GetParameters(object o)
        {
            return GetParameters(o.GetType())
                .Select(fi => (VolumeParameter)fi.GetValue(o))
                .ToArray();
        }

        static Dictionary<System.Type, FieldInfo[]> sTypeFieldMapCache = new();
        
        public static FieldInfo[] GetParameters(System.Type t)
        {
            if (!sTypeFieldMapCache.TryGetValue(t, out var parameters))
            {
                parameters = sTypeFieldMapCache[t] = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(fi => fi.FieldType.IsSubclassOf(typeof(VolumeParameter)))
                    .OrderBy(fi => fi.MetadataToken)
                    .ToArray();
            }
            return parameters;
        }

        public QualitySettingsComponent()
        {
            foreach (var parameter in GetParameters(GetType()))
            {
                parameter.SetValue(this, System.Activator.CreateInstance(parameter.FieldType));
            }
        }
        
        public virtual void ApplyDefaultValues() {}

        public QualitySettingsComponent Clone()
        {
            var clone = (QualitySettingsComponent)MemberwiseClone();
            foreach (var parameter in GetParameters(clone.GetType()))
            {
                var sourceParam = (System.ICloneable)parameter.GetValue(clone);
                parameter.SetValue(clone, sourceParam.Clone());
            }
            return clone;
        }

        public bool GetAllOverrides()
        {
            var overrideState = true;
            foreach (var parameter in GetParameters())
                overrideState &= parameter.overrideState;
            return overrideState;
        }

        public void SetAllOverrides(bool overrideState)
        {
            foreach (var parameter in GetParameters())
                parameter.overrideState = overrideState;
        }

        public void CopyFrom(QualitySettingsComponent source)
        {
            if (source.GetType() != GetType())
                throw new System.ArgumentException();
                    
            var selfParams = GetParameters();
            var sourceParams = source.GetParameters();

            for (var i = 0; i < selfParams.Length; ++i)
            {
                selfParams[i].SetValue(sourceParams[i]);
                selfParams[i].overrideState = sourceParams[i].overrideState;
            }
        }
        
        public void OverrideFrom(QualitySettingsComponent source)
        {
            if (source.GetType() != GetType())
                throw new System.ArgumentException();
                    
            var selfParams = GetParameters();
            var sourceParams = source.GetParameters();

            for (var i = 0; i < selfParams.Length; ++i)
            {
                if (sourceParams[i].overrideState)
                    selfParams[i].SetValue(sourceParams[i]);
            }
        }
    }
    
    [System.Flags]
    public enum BuildTarget
    {
        Master = 0,

        EditorOnly = 1 << 0,
        
        PC = 1 << 1,
        Mac = 1 << 2,
        
        Playstation5 = 1 << 3,

        XboxSeriesS = 1 << 4,
        XboxSeriesX = 1 << 5,
    }
    
    public abstract class QualityComponentsBag
    {
        public string name;
        public BuildTarget buildTargets;

        public QualityComponentsBag()
        {
            foreach (var component in GetComponents())
                component.SetValue(this, System.Activator.CreateInstance(component.FieldType));
        }
        
        public virtual void ApplyDefaultValues()
        {
            name = "Master";
            buildTargets = BuildTarget.Master;

            foreach (var component in GetComponents())
                ((QualitySettingsComponent)component.GetValue(this)).ApplyDefaultValues();
        }

        public QualityComponentsBag Clone()
        {
            var clone = (QualityComponentsBag)MemberwiseClone();
            foreach (var component in GetComponents())
            {
                var cloneComponent = (QualitySettingsComponent) component.GetValue(clone);
                component.SetValue(clone, cloneComponent.Clone());
            }
            return clone;
        }

        public void SetAllOverrides(bool overrideState)
        {
            foreach (var component in GetComponents())
            {
                var selfComponent = (QualitySettingsComponent)component.GetValue(this);
                selfComponent.SetAllOverrides(overrideState);
            }
        }

        public void CopyFrom(QualityComponentsBag source)
        {
            foreach (var component in GetComponents())
            {
                var selfComponent = (QualitySettingsComponent)component.GetValue(this);
                var sourceComponent = (QualitySettingsComponent) component.GetValue(source);
                selfComponent.CopyFrom(sourceComponent);
            }
        }

        public void OverrideFrom(QualityComponentsBag source)
        {
            foreach (var component in GetComponents())
            {
                var selfComponent = (QualitySettingsComponent)component.GetValue(this);
                var sourceComponent = (QualitySettingsComponent) component.GetValue(source);
                selfComponent.OverrideFrom(sourceComponent);
            }
        }

        protected abstract FieldInfo[] GetComponents();

        public static FieldInfo[] GetComponents(System.Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(fi => fi.FieldType.IsSubclassOf(typeof(QualitySettingsComponent)))
                .OrderBy(fi => fi.MetadataToken)
                .ToArray();
        }
    }
}
