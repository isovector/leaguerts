using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LeagueEngine.Effects {
    public enum fowEffectTechniques {
        Unit,
        Selection,
        Terrain,
    }

    public class fowEffect {
        private Effect _baseEffect;
        public Effect BaseEffect {
            get { return _baseEffect; }
        }

        private string _effectName;
        public string EffectName {
            get { return _effectName; }
        }

        #region Effect Parameters

        private EffectParameter _viewParam;
        public Matrix View {
            get {
                if (_viewParam == null)
                    throw new Exception("Cannot get value of View; View EffectParameter is null.");
                return _viewParam.GetValueMatrix();
            }
            set {
                if (_viewParam == null)
                    throw new Exception("Cannot set value of View; View EffectParameter is null.");
                _viewParam.SetValue(value);
            }
        }

        private EffectParameter _projParam;
        public Matrix Proj {
            get {
                if (_projParam == null)
                    throw new Exception("Cannot get value of Proj; Proj EffectParameter is null.");
                return _projParam.GetValueMatrix();
            }
            set {
                if (_projParam == null)
                    throw new Exception("Cannot set value of Proj; Proj EffectParameter is null.");
                _projParam.SetValue(value);
            }
        }

        private EffectParameter _worldParam;
        public Matrix World {
            get {
                if (_worldParam == null)
                    throw new Exception("Cannot get value of World; World EffectParameter is null.");
                return _worldParam.GetValueMatrix();
            }
            set {
                if (_worldParam == null)
                    throw new Exception("Cannot set value of World; World EffectParameter is null.");
                _worldParam.SetValue(value);
            }
        }

        private EffectParameter _visibilityParam;
        public float Visibility {
            get {
                if (_visibilityParam == null)
                    throw new Exception("Cannot get value of Visibility; Visibility EffectParameter is null.");
                return _visibilityParam.GetValueSingle();
            }
            set {
                if (_visibilityParam == null)
                    throw new Exception("Cannot set value of Visibility; Visibility EffectParameter is null.");
                _visibilityParam.SetValue(value);
            }
        }

        private EffectParameter _colorParam;
        public Vector3 Color {
            get {
                if (_colorParam == null)
                    throw new Exception("Cannot get value of Color; Color EffectParameter is null.");
                return _colorParam.GetValueVector3();
            }
            set {
                if (_colorParam == null)
                    throw new Exception("Cannot set value of Color; Color EffectParameter is null.");
                _colorParam.SetValue(value);
            }
        }

        private EffectParameter _ambientParam;
        public float Ambient {
            get {
                if (_ambientParam == null)
                    throw new Exception("Cannot get value of Ambient; Ambient EffectParameter is null.");
                return _ambientParam.GetValueSingle();
            }
            set {
                if (_ambientParam == null)
                    throw new Exception("Cannot set value of Ambient; Ambient EffectParameter is null.");
                _ambientParam.SetValue(value);
            }
        }

        private EffectParameter _lightDirParam;
        public Vector3 LightDir {
            get {
                if (_lightDirParam == null)
                    throw new Exception("Cannot get value of LightDir; LightDir EffectParameter is null.");
                return _lightDirParam.GetValueVector3();
            }
            set {
                if (_lightDirParam == null)
                    throw new Exception("Cannot set value of LightDir; LightDir EffectParameter is null.");
                _lightDirParam.SetValue(value);
            }
        }

        private EffectParameter _enableLightParam;
        public bool EnableLight {
            get {
                if (_enableLightParam == null)
                    throw new Exception("Cannot get value of EnableLight; EnableLight EffectParameter is null.");
                return _enableLightParam.GetValueBoolean();
            }
            set {
                if (_enableLightParam == null)
                    throw new Exception("Cannot set value of EnableLight; EnableLight EffectParameter is null.");
                _enableLightParam.SetValue(value);
            }
        }

        private EffectParameter _FOWTextureParam;
        public Texture2D FOWTexture {
            get {
                if (_FOWTextureParam == null)
                    throw new Exception("Cannot get value of FOWTexture; FOWTexture EffectParameter is null.");
                return _FOWTextureParam.GetValueTexture2D();
            }
            set {
                if (_FOWTextureParam == null)
                    throw new Exception("Cannot set value of FOWTexture; FOWTexture EffectParameter is null.");
                _FOWTextureParam.SetValue(value);
            }
        }

        private EffectParameter _TerrainTextureParam;
        public Texture2D TerrainTexture {
            get {
                if (_TerrainTextureParam == null)
                    throw new Exception("Cannot get value of TerrainTexture; TerrainTexture EffectParameter is null.");
                return _TerrainTextureParam.GetValueTexture2D();
            }
            set {
                if (_TerrainTextureParam == null)
                    throw new Exception("Cannot set value of TerrainTexture; TerrainTexture EffectParameter is null.");
                _TerrainTextureParam.SetValue(value);
            }
        }

        private EffectParameter _UnitTextureParam;
        public Texture2D UnitTexture {
            get {
                if (_UnitTextureParam == null)
                    throw new Exception("Cannot get value of UnitTexture; UnitTexture EffectParameter is null.");
                return _UnitTextureParam.GetValueTexture2D();
            }
            set {
                if (_UnitTextureParam == null)
                    throw new Exception("Cannot set value of UnitTexture; UnitTexture EffectParameter is null.");
                _UnitTextureParam.SetValue(value);
            }
        }

        private EffectParameter _CliffTextureParam;
        public Texture2D CliffTexture {
            get {
                if (_CliffTextureParam == null)
                    throw new Exception("Cannot get value of UnitTexture; UnitTexture EffectParameter is null.");
                return _CliffTextureParam.GetValueTexture2D();
            }
            set {
                if (_CliffTextureParam == null)
                    throw new Exception("Cannot set value of UnitTexture; UnitTexture EffectParameter is null.");
                _CliffTextureParam.SetValue(value);
            }
        }





        #endregion

        #region Effect Techniques

        private EffectTechnique _UnitTechnique;

        private EffectTechnique _SelectionTechnique;

        private EffectTechnique _TerrainTechnique;

        #endregion


        public fowEffect(string effectName) {
            _effectName = effectName;
        }

        public void Load(ContentManager contentManager) {
            _baseEffect = contentManager.Load<Effect>(_effectName);

            _viewParam = _baseEffect.Parameters["view"];
            _projParam = _baseEffect.Parameters["proj"];
            _worldParam = _baseEffect.Parameters["world"];
            _visibilityParam = _baseEffect.Parameters["visibility"];
            _colorParam = _baseEffect.Parameters["color"];
            _ambientParam = _baseEffect.Parameters["ambient"];
            _lightDirParam = _baseEffect.Parameters["lightDir"];
            _enableLightParam = _baseEffect.Parameters["enableLight"];
            _FOWTextureParam = _baseEffect.Parameters["FOWTexture"];
            _TerrainTextureParam = _baseEffect.Parameters["TerrainTexture"];
            _UnitTextureParam = _baseEffect.Parameters["UnitTexture"];
            _CliffTextureParam = _baseEffect.Parameters["CliffTexture"];

            _UnitTechnique = _baseEffect.Techniques["Unit"];
            _SelectionTechnique = _baseEffect.Techniques["Selection"];
            _TerrainTechnique = _baseEffect.Techniques["Terrain"];
        }

        public void SetCurrentTechnique(fowEffectTechniques technique) {
            switch (technique) {
                case fowEffectTechniques.Unit:
                    _baseEffect.CurrentTechnique = _UnitTechnique;
                    break;

                case fowEffectTechniques.Selection:
                    _baseEffect.CurrentTechnique = _SelectionTechnique;
                    break;

                case fowEffectTechniques.Terrain:
                    _baseEffect.CurrentTechnique = _TerrainTechnique;
                    break;

            }
        }
    }
}
