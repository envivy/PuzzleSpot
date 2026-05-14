using BX;
using I2.Loc;

public class ReviewUI : UI
{

	public override void ShowUI()
	{
		base.ShowUI();
		GameSet.instance.LogEvent("Rate_Show");
		GameSet.instance.userData.IsShowReview = true;
		GameSet.instance.SaveUserData();
	}
	public void OneStarClick() {
		GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.BtnAudio);
		GameSet.instance.gameManager.ShowToast(LocalizationManager.GetTermTranslation("ThanksFeedBack"));
		GameSet.instance.LogEvent("Rate_Click_1");
		CloseMe();
	}
	public void TwoStarClick()
	{
		GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.BtnAudio);
		GameSet.instance.gameManager.ShowToast(LocalizationManager.GetTermTranslation("ThanksFeedBack"));
		GameSet.instance.LogEvent("Rate_Click_2");
		CloseMe();
	}
	public void ThreeStarClick()
	{
		GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.BtnAudio);
		GameSet.instance.gameManager.ShowToast(LocalizationManager.GetTermTranslation("ThanksFeedBack"));
		GameSet.instance.LogEvent("Rate_Click_3");
		CloseMe();
	}
	public void FourStarClick()
	{
		GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.BtnAudio);
		GameSet.instance.gameManager.ShowToast(LocalizationManager.GetTermTranslation("ThanksFeedBack"));
		GameSet.instance.LogEvent("Rate_Click_4");
		CloseMe();
	}

	public void FiveStarClick()
	{
		GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.BtnAudio);
		BXSdk.Instance.Review();
		GameSet.instance.LogEvent("Rate_Click_5");
		CloseMe();
	}

	public void CloseClick() {
		GameSet.instance.LogEvent("Rate_Close");
		CloseMe();
	}

	public override void CloseMe()
	{
		base.CloseMe();
	}
}
