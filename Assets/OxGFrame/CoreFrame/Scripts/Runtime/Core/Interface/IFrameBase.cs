using Cysharp.Threading.Tasks;

namespace OxGFrame.CoreFrame
{
    public interface IFrameBase
    {
        void OnCreate();

        void InitFirst();

        UniTask PreInit();

        void Display(object obj);

        void Hide(bool disabledPreClose);

        void OnRelease();

        void SetNames(string assetName);

        void SetGroupId(int id);

        void SetHidden(bool isHidden);
    }
}