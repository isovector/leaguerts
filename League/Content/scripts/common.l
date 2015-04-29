float ElapsedTime = 0f;

on Update: void Common_UpdateElapsed(GameTime gameTime) {
    ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
}

void Call(ScriptingEngine.AsyncCall f) {
     Script.StartCallback(f);
}

void Wait(float f) {
     Thread.Sleep((int)(f * 1000));
}