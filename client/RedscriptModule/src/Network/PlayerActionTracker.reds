module CyberM.Network.Managers

public native class PlayerActionTracker {
    public native func RecordPlayerAction(actionName: CName, actionType: gameinputActionType,  value: Float) -> Void;
    public native func OnShoot(event: ref<gameprojectileShootEvent>) -> Void;
    public native func OnHit(gameObject: ref<GameObject>, event: ref<gameHitEvent>);
}
