using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LeagueEngine.Effects {
    public enum CooldownEffectTechniques {
        Cooldown,
    }

    public class CooldownEffect {
        private Effect _baseEffect;
        public Effect BaseEffect {
            get { return _baseEffect; }
        }

        private string _effectName;
        public string EffectName {
            get { return _effectName; }
        }

        #region Effect Parameters

        private EffectParameter _WorldParam;
        public Matrix World {
            get {
                if (_WorldParam == null)
                    throw new Exception("Cannot get value of World; World EffectParameter is null.");
                return _WorldParam.GetValueMatrix();
            }
            set {
                if (_WorldParam == null)
                    throw new Exception("Cannot set value of World; World EffectParameter is null.");
                _WorldParam.SetValue(value);
            }
        }

        private EffectParameter _ViewParam;
        public Matrix View {
            get {
                if (_ViewParam == null)
                    throw new Exception("Cannot get value of View; View EffectParameter is null.");
                return _ViewParam.GetValueMatrix();
            }
            set {
                if (_ViewParam == null)
                    throw new Exception("Cannot set value of View; View EffectParameter is null.");
                _ViewParam.SetValue(value);
            }
        }

        private EffectParameter _ProjectionParam;
        public Matrix Projection {
            get {
                if (_ProjectionParam == null)
                    throw new Exception("Cannot get value of Projection; Projection EffectParameter is null.");
                return _ProjectionParam.GetValueMatrix();
            }
            set {
                if (_ProjectionParam == null)
                    throw new Exception("Cannot set value of Projection; Projection EffectParameter is null.");
                _ProjectionParam.SetValue(value);
            }
        }

        private EffectParameter _CompleteParam;
        public float Complete {
            get {
                if (_CompleteParam == null)
                    throw new Exception("Cannot get value of Complete; Complete EffectParameter is null.");
                return _CompleteParam.GetValueSingle();
            }
            set {
                if (_CompleteParam == null)
                    throw new Exception("Cannot set value of Complete; Complete EffectParameter is null.");
                _CompleteParam.SetValue(value);
            }
        }

        #endregion

        #region Effect Techniques

        private EffectTechnique _CooldownTechnique;

        #endregion


        public CooldownEffect(string effectName) {
            _effectName = effectName;
        }

        public void Load(ContentManager contentManager) {
            _baseEffect = contentManager.Load<Effect>(_effectName);

            _WorldParam = _baseEffect.Parameters["World"];
            _ViewParam = _baseEffect.Parameters["View"];
            _ProjectionParam = _baseEffect.Parameters["Projection"];
            _CompleteParam = _baseEffect.Parameters["Complete"];

            _CooldownTechnique = _baseEffect.Techniques["Cooldown"];
        }

        public void SetCurrentTechnique(CooldownEffectTechniques technique) {
            switch (technique) {
                case CooldownEffectTechniques.Cooldown:
                    _baseEffect.CurrentTechnique = _CooldownTechnique;
                    break;

            }
        }
    }
}
