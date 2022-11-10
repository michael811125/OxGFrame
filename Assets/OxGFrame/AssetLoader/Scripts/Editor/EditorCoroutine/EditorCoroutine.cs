using System.Collections;

public class EditorCoroutine
{
    private IEnumerator _routine;

    private EditorCoroutine(IEnumerator routine)
    {
        this._routine = routine;
    }

    public static EditorCoroutine Start(IEnumerator routine)
    {
        EditorCoroutine coroutine = new EditorCoroutine(routine);
        coroutine.Start();
        return coroutine;
    }

    public void Start()
    {
        UnityEditor.EditorApplication.update += this.Update;
    }

    public void Stop()
    {
        UnityEditor.EditorApplication.update -= this.Update;
    }

    public void Update()
    {
        if (!this._routine.MoveNext()) this.Stop();
    }
}