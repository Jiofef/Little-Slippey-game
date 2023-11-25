using Godot;
using System;

public partial class MISSINGNO : AnimatedSprite2D
{
	Sprite2D _sprite2D;
	AnimationPlayer _glitchPlayer;
	Random _random = new Random();
	private readonly string[] _spritePathes = {
        "Crosses/BlueElementalCrossPart","Crosses/BlumAbortButton","Crosses/BlumCross","Crosses/BlumCrossWarning","Crosses/CannonBall","Crosses/CannonBarrel","Crosses/CannonWheel","Crosses/DefaultCross","Crosses/DefaultCrossWarning","Crosses/ElementalCross","Crosses/EnhancedBlumCross","Crosses/EnhancedBlumCrossCrystalAnimation",  "Crosses/EnhancedBlumCrossCrystalHead","Crosses/EnhancedBlumCrossEnergyBeamAnimation","Crosses/EnhancedBlumCrossWarning","Crosses/EnhancedDefaultCross","Crosses/EnhancedDefaultCrossWarning","Crosses/EnhancedElementalCrossAnimation","Crosses/EnhancedRestlessCross","Crosses/EnhancedRestlessCrossWarning",
        "Crosses/GreenElementalCrossPartVar1","Crosses/GreenElementalCrossPartVar2","Crosses/GreenElementalCrossPartVar3","Crosses/GreenElementalCrossPartVar4","Crosses/GreenElementCrossVineAnimation","Crosses/Helicopter","Crosses/HelicopterBomb","Crosses/RedElementalCrossPart","Crosses/RestlessCross","Crosses/RestlessCrossWarning",
        "Crosses/TornCannonBarrel","Interface/AcceptButton","Interface/AchievementBox","Interface/AchievementsIcon","Interface/AchievementsMenu","Interface/CancelButton","Interface/LevelIconButton","Interface/LevelIconButtonDisabled","Interface/LevelListScrollButtonDisabled","Interface/LevelListScrollButtonLeft",
        "Interface/LevelListScrollButtonRight","Interface/LevelsIcons","Interface/LevelsMenuBox","Interface/LevelsMenuIcon","Interface/LevelsMenuLevel5IconWindow","Interface/MenuBackground","Interface/MenuBackgroundPattern","Interface/MicroLockSprite","Interface/OptionsIcon","Interface/OptionsMenuBox",
        "Interface/OptionsMenuVideoContainer","Interface/PauseButtonsFrame","Interface/PauseMenuButton","Interface/PauseOptionsButton","Interface/PauseRewindButton","Interface/SkinButton","Interface/SkinButtonDisabled","Interface/SkinsMenuBox","Interface/SkinsMenuIcon","Interface/TheFinalLetter","Interface/TitlesIcon",
        "Interface/UnpauseButton","Levels/Level1/BackClouds","Levels/Level1/Clouds","Levels/Level1/DirtParticles","Levels/Level1/FrontLayer","Levels/Level1/RockyDirtParticles","Levels/Level1/Sky","Levels/Level1/SlSlLSLLSLSLlsLSLLSslsl","Levels/Level1/StreamAnimation","Levels/Level1/SunRaysAnimation","Levels/Level1/TheConsciousness",
        "Levels/Level1/TheMatter","Levels/Level1/TheRelativity","Levels/Level1/TutorialBackClouds","Levels/Level1/TutorialBackLayer","Levels/Level1/TutorialClouds","Levels/Level1/TutorialTV","Levels/Level1/TutorialTVBangerMovie","Levels/Level1/TutorialTVHaloAnimation","Levels/Level1/TutorialTVWings","Levels/Level1/WoodParticles",
        "Levels/Level1/xxlman","Levels/Level2/BackClouds","Levels/Level2/Clouds","Levels/Level2/Combine","Levels/Level2/FieldBackLayer","Levels/Level2/FieldFrontLayer","Levels/Level2/FieldMiddleLayer","Levels/Level2/ShOatAnimation","Levels/Level2/Sky","Levels/Level2/SmokeAnimation","Levels/Level2/wtf","Levels/Level3/Clouds",
        "Levels/Level3/Sea","Levels/Level3/Sky","Levels/Level4/Background","Levels/Level4/BigSaw","Levels/Level4/BigSawStud","Levels/Level4/DestroyedWall","Levels/Level4/SawHolder","Levels/Level4/Stud",
        "Levels/Level4/TheMostOrdinaryUnremarkableSawWhichSurprisinglyCanKillAPlayerWithoutProblemsBecauseItHasAHitboxOnTheSameLayerAsTheExplosionOfCrossesThatsInShort","Levels/Level4/UltraBigSaw","Levels/Level4/UltraBigSawStud","Levels/Level5/HomeSweetHome","Levels/Level5/Sky","Levels/Level6/BackCity","Levels/Level6/FrontCity",
        "Levels/Level6/GieroFlyingCar","Levels/Level6/MiddleCity","Levels/Level6/Sky","Levels/Level7/BackLayer","Levels/Level7/FrontLayer","Levels/Level7/MindTumor","Levels/Level8/AcidAndLavaTileset","Levels/Level8/BackLayer","Levels/Level8/Button","Levels/Level8/CapsuleObjects","Levels/Level8/CompressEyesAnimation",
        "Levels/Level8/CompressFleshAnimation","Levels/Level8/CompressHost","Levels/Level8/CompressMouthsAnimation","Levels/Level8/CompressPimplesAnimation","Levels/Level8/CompressProjectorAnimation","Levels/Level8/CompressTileMap","Levels/Level8/FrontLayer","Levels/Level8/Gatcha","Levels/Level8/Gotcha",
        "Levels/Level8/LaboratoryCapsules","Levels/Level8/Projection","Levels/Level9/BullDashZone","Levels/Level9/CornerShadow","Levels/Level9/DAaAAaAaArk","Levels/Level9/DoomApproachesAnimation","Levels/Level9/FOCUSPOCUS","Levels/Level9/Hell","Levels/Level9/HellGround","Levels/Level9/HellHorizon",
        "Levels/Level9/HellVolcanoSmokeAnimation","Levels/Level9/JiofefEye","Levels/Level9/JiofefHandSpriteList","Levels/Level9/JiofefHeadSpriteList","Levels/Level9/NoWayBro","Levels/Level9/OpticNerveAnimation","Levels/Level9/SpitCross","Levels/Level9/SpitCrossWarning","Levels/Level9/Volcano","Levels/Level9/VolcanoInterior",
        "Levels/Level9/VolcanoStalactites","Levels/Level9/VulcanInteriorLavaAnimation","Levels/Level9/WhyDontYouSleep","Levels/Level10/BackCloudLayer","Levels/Level10/BackPipes","Levels/Level10/Clouds","Levels/Level10/CoolEasterEgg","Levels/Level10/ForePipes","Levels/Level10/FrontCloudLayer","Levels/Level10/MiddleCloudLayer",
        "Levels/Level10/PortableControlledDroneCapsule","Levels/Level10/Rail","Levels/Level10/Sky","Levels/Level10/TestChamber","Levels/ANineGreetingsTileset","Levels/BaseTileSet","Levels/DropesAndHeamsTileset","Levels/FallenDawnTileset","Levels/FuckItTileset","Levels/FuckItVolume2Tileset","Levels/InMyHeadTileset",
        "Levels/Mort'sTileset","Levels/NePeretrudilsyaTileset","Levels/poliSiSagEtilEseT","Levels/WhyIsItStillRainingTileset","Levels/WorkingPleaseWaitTileset","Other/AchievementStarParticle","Other/Explosion","Other/Gag","Other/Jiofef","Other/JioYobaFefskiArtefact","Other/MySelf-Esteem","Other/Note","Other/OblivionNoise",
        "Other/PlayerGhost","Other/Shine","Other/Shoesofef","Other/SleepyZ","PlayerSkins/Bondey","PlayerSkins/CompressPile","PlayerSkins/Daley","PlayerSkins/Hostey","PlayerSkins/JioYobaFefskiHand","PlayerSkins/JioYobaFefskiHead","PlayerSkins/JioYobaFefskiLeg","PlayerSkins/Pineplum","PlayerSkins/Samey","PlayerSkins/Sanboy",
        "PlayerSkins/Sleepy","PlayerSkins/Slippey","PlayerSkins/SlippeyChad","PlayerSkins/Strawman"
};
	

    private readonly string[] _animationPlayerAnimations = {"RegionGlitch1", "RegionGlitch2", "RegionGlitch3"};
    private int _randomInterval = 5;
	public override void _Ready()
	{
		_sprite2D = GetNode<Sprite2D>("Sprite2D");
        _glitchPlayer = GetNode<AnimationPlayer>("GlitchPlayer");
	}

	public override void _Process(double delta)
	{
		if (_random.Next(_randomInterval) == 0)
		{
            _sprite2D.Texture = ResourceLoader.Load<Texture2D>("res://Content/Sprites/" + _spritePathes[_random.Next(_spritePathes.Length)] + ".png");
            _sprite2D.RegionRect = new Rect2(_random.Next(_sprite2D.Texture.GetWidth()), _random.Next(_sprite2D.Texture.GetHeight()), 16 + _random.Next(-4, 4), 16 + _random.Next(-4, 4));
        }
		if (_random.Next(100 - _randomInterval) == 0)
			_randomInterval = _random.Next(25);
		if (_random.Next(_randomInterval * 3) == 0)
			Modulate = new Color((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble());
		if (_random.Next(300 + _randomInterval) == 3)
            _glitchPlayer.Play(_animationPlayerAnimations[_random.Next(_animationPlayerAnimations.Length)]);
    }
	public void GlitchedAnimationFinished(string animationName)
	{
		if (_random.Next(5 + _randomInterval / 4 + _random.Next(_randomInterval / 5 * (animationName == "RegionGlitched1" ? 10 : 2))) != 0)
            _glitchPlayer.Play(_animationPlayerAnimations[_random.Next(_animationPlayerAnimations.Length)]);
	}
}
