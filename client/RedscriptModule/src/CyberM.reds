import CyberM.Network.Managers.*
//import Codeware.*

@addField(DebugDataDef)
public let AutoContinueUsed: Bool;

@wrapMethod(SingleplayerMenuGameController)
protected cb func OnSavesForLoadReady(saves: array<String>) -> Bool {
	let res = wrappedMethod(saves);

    let handler = this.GetSystemRequestsHandler();
    if (!handler.IsPreGame()) {
        return res; // PreGame == MainMenu
    }

    // TODO: In case the connection is too slow, we miss this. What to do then? Should the DLL somehow check the saves and then LoadLastCheckpoint?
    //  technically, it could still hook this method. Or put hooking aside, we could call into it and even pass the SystemRequestsHandler.
	if this.m_savesCount > 0 && !GetAllBlackboardDefs().DebugData.AutoContinueUsed {
        let networkSystem = GameInstance.GetNetworkGameSystem();
        if (networkSystem.FullyConnected) {
		    handler.LoadLastCheckpoint(false);
        } else {
            networkSystem.EnqueueLoadLastCheckpoint(handler);
        }
	}
}

@wrapMethod(SingleplayerMenuGameController)
protected cb func OnUninitialize() -> Bool {
	wrappedMethod();
	GetAllBlackboardDefs().DebugData.AutoContinueUsed = true;
}

@wrapMethod(SingleplayerMenuGameController)
private func PopulateMenuItemList() -> Void {
    wrappedMethod();
    this.AddMenuItem("Cyber-M", n"OnBuyGame");
}

@wrapMethod(PlayerPuppet)
protected cb func OnAction(action: ListenerAction, consumer: ListenerActionConsumer) -> Bool {
    wrappedMethod(action, consumer);
    let name = ListenerAction.GetName(action);
    let type = ListenerAction.GetType(action);
    let value = ListenerAction.GetValue(action);

    GameInstance.GetNetworkGameSystem().playerActionTracker.RecordPlayerAction(name, type, value);

    // if (StrCmp(NameToString(name), "Jump") == 0 && StrCmp(EnumValueToString("gameinputActionType", Cast(EnumInt(type))), "BUTTON_RELEASED") == 0) {
    //     let npcSpec = new DynamicEntitySpec();
    //     //npcSpec.recordID = t"Character.spr_animals_bouncer1_ranged1_omaha_mb";
    //     npcSpec.recordID = t"Character.Panam";
    //     npcSpec.appearanceName = n"random";

    //     // base\characters\entities\main_npc\panam.ent
    //     //npcSpec.recordID = t"Vehicle.v_sport2_quadra_type66";
    //     //npcSpec.appearanceName = n"quadra_type66__basic_bulleat";
    //     npcSpec.appearanceName = n"random";
    //     npcSpec.position = this.GetWorldPosition();//(4.0, -45.0);
    //     npcSpec.orientation = this.GetWorldOrientation();//(-40.0);
    //     npcSpec.persistState = false;
    //     npcSpec.persistSpawn = false;
    //     npcSpec.tags = [n"MyMod"];

    //     GameInstance.GetDynamicEntitySystem().CreateEntity(npcSpec);
    // }
}

@wrapMethod(BaseProjectile)
protected cb func OnShoot(eventData: ref<gameprojectileShootEvent>) -> Bool {
    wrappedMethod(eventData);
    FTLog("Shoot");
    GameInstance.GetNetworkGameSystem().playerActionTracker.OnShoot(eventData);
}

@wrapMethod(BaseProjectile)
protected cb func OnShootTarget(eventData: ref<gameprojectileShootTargetEvent>) -> Bool {
    wrappedMethod(eventData);
    FTLog("Shoot Target");
}

@wrapMethod(GameObject)
protected cb func OnHit(evt: ref<gameHitEvent>) -> Bool {
    wrappedMethod(evt);
    FTLog("OnHit");
    GameInstance.GetNetworkGameSystem().playerActionTracker.OnHit(this, evt);
}