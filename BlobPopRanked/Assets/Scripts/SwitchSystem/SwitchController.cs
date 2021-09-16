using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public SwitchSettings SwitchSettings;

    void Start()
    {
        EventBus._.On(Evt.SWITCH_BLOBS, () =>
        {
            switchBlob();
        });

        EventBus._.On(Evt.MAKE_ANOTHER_BLOB, () =>
        {
            makeAnotherBlob();
        });
    }

    void switchBlob()
    {
        SwitchBlobEvent data = new SwitchBlobEvent(true);
        EventBus._.Emit(Evt.STOP_SHOOTING, data);


        moveSwitchableToShootable();
        moveShootableToSwitchable();
        switchThemUp();

        Timer._.InternalWait(() =>
        {
            data.IsSwitchInProgress = false;
            EventBus._.Emit(Evt.STOP_SHOOTING, data);
        }, SwitchSettings.SwitchTime);
    }

    void moveSwitchableToShootable()
    {
        Game._.Player.SecondProjectile.transform.position = SwitchSettings.ShootableBlobPosition;
        Game._.Player.SetupProjectileBlob(ref Game._.Player.SecondProjectile);
    }

    void moveShootableToSwitchable()
    {
        Game._.Player.FirstProjectile.transform.position = SwitchSettings.SwitchableBlobPosition;
    }

    void switchThemUp()
    {
        BlobProjectile previousShootable = Game._.Player.FirstProjectile;
        Game._.Player.FirstProjectile = Game._.Player.SecondProjectile;
        Game._.Player.SecondProjectile = previousShootable;
    }

    void makeAnotherBlob()
    {
        moveSwitchableToShootable();

        Timer._.InternalWait(() =>
        {
            Game._.Player.FirstProjectile = Game._.Player.SecondProjectile;

            Game._.Player.SecondProjectile = Game._.Player.GetRandomBlob();
            Game._.Player.SecondProjectile.transform.position = SwitchSettings.SwitchableBlobPosition;

            Game._.Player.MakingBlob = false;
            Game._.Player.BlobInMotion = false;
            UIController._.UiPointerArea.SetActive(true);

        }, SwitchSettings.MakeShootableBlobDebounceTime);
    }
}
