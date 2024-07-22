

namespace Timba.Packages.Games.PlayerControllerModule
{
    public interface IInputHandleable
    {
        public void Init();

        public float GetHorizontalInput();

        public float GetVerticalInput();

    }

}
