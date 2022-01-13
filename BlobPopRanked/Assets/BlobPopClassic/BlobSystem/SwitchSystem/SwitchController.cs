using Assets.BlobPopClassic;
using Assets.BlobPopClassic.Blobs;
using Assets.HeadStart.Core;
using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public SwitchSettings SwitchSettings;
    private BlobPopPlayer _player;

    void Start()
    {
        __.Event.On(Evt.SWITCH_BLOBS, () =>
        {
            switchBlob();
        });

        __.Event.On(Evt.MAKE_ANOTHER_BLOB, () =>
        {
            makeAnotherBlob();
        });
    }

    public void Init(BlobPopPlayer player)
    {
        _player = player;
    }

    void switchBlob()
    {
        SwitchBlobEvent data = new SwitchBlobEvent(true);
        __.Event.Emit(Evt.STOP_SHOOTING, data);


        moveSwitchableToShootable();
        moveShootableToSwitchable();
        switchThemUp();

        __.Time.RxWait(() =>
        {
            data.IsSwitchInProgress = false;
            __.Event.Emit(Evt.STOP_SHOOTING, data);
        }, SwitchSettings.SwitchTime);
    }

    void moveSwitchableToShootable()
    {
        _player.SecondProjectile.transform.position = SwitchSettings.ShootableBlobPosition;
        _player.SetupProjectileBlob(ref _player.SecondProjectile);
    }

    void moveShootableToSwitchable()
    {
        _player.FirstProjectile.transform.position = SwitchSettings.SwitchableBlobPosition;
    }

    void switchThemUp()
    {
        BlobProjectile previousShootable = _player.FirstProjectile;
        _player.FirstProjectile = _player.SecondProjectile;
        _player.SecondProjectile = previousShootable;
    }

    void makeAnotherBlob()
    {
        moveSwitchableToShootable();

        __.Time.RxWait(() =>
        {
            _player.FirstProjectile = _player.SecondProjectile;

            _player.SecondProjectile = _player.GetRandomBlob();
            _player.SecondProjectile.transform.position = SwitchSettings.SwitchableBlobPosition;

            _player.MakingBlob = false;
            _player.BlobInMotion = false;
            
            __.Event.Emit(Evt.ACTIVATE_POINTER_AREA, true);

        }, SwitchSettings.MakeShootableBlobDebounceTime);
    }
}
