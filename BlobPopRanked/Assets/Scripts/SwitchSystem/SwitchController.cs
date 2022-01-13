using Assets.HeadStart.Core;
using Assets.HeadStart.Core.Player;
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

        __.Time.RxWait(() =>
        {
            data.IsSwitchInProgress = false;
            EventBus._.Emit(Evt.STOP_SHOOTING, data);
        }, SwitchSettings.SwitchTime);
    }

    void moveSwitchableToShootable()
    {
        (Main._.Game.Player as BlobPopPlayer).SecondProjectile.transform.position = SwitchSettings.ShootableBlobPosition;
        (Main._.Game.Player as BlobPopPlayer).SetupProjectileBlob(ref (Main._.Game.Player as BlobPopPlayer).SecondProjectile);
    }

    void moveShootableToSwitchable()
    {
        (Main._.Game.Player as BlobPopPlayer).FirstProjectile.transform.position = SwitchSettings.SwitchableBlobPosition;
    }

    void switchThemUp()
    {
        BlobProjectile previousShootable = (Main._.Game.Player as BlobPopPlayer).FirstProjectile;
        (Main._.Game.Player as BlobPopPlayer).FirstProjectile = (Main._.Game.Player as BlobPopPlayer).SecondProjectile;
        (Main._.Game.Player as BlobPopPlayer).SecondProjectile = previousShootable;
    }

    void makeAnotherBlob()
    {
        moveSwitchableToShootable();

        __.Time.RxWait(() =>
        {
            (Main._.Game.Player as BlobPopPlayer).FirstProjectile = (Main._.Game.Player as BlobPopPlayer).SecondProjectile;

            (Main._.Game.Player as BlobPopPlayer).SecondProjectile = (Main._.Game.Player as BlobPopPlayer).GetRandomBlob();
            (Main._.Game.Player as BlobPopPlayer).SecondProjectile.transform.position = SwitchSettings.SwitchableBlobPosition;

            (Main._.Game.Player as BlobPopPlayer).MakingBlob = false;
            (Main._.Game.Player as BlobPopPlayer).BlobInMotion = false;
            // TODO - refactor this
            // UIController._.UiPointerArea.SetActive(true);

        }, SwitchSettings.MakeShootableBlobDebounceTime);
    }
}
