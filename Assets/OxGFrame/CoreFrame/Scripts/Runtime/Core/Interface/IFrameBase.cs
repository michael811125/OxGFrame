using Cysharp.Threading.Tasks;

namespace OxGFrame.CoreFrame
{
    public interface IFrameBase
    {
        void BeginInit();

        void InitFirst();

        UniTask PreInit();

        void Display(object obj);

        void Hide(bool disableDoSub);

        void OnRelease();

        void SetNames(string assetName);

        void SetGroupId(int id);

        void SetHidden(bool isHidden);
    }
}