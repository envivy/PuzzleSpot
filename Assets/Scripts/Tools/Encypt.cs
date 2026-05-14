
public class Encypt
{
    static byte[] mask = { 55,105,32,86};
    public static void DealData(byte[]fileData) {
        int maskIndex = 0;
        for (int i = 0; i < fileData.Length; i++)
        {
            fileData[i] = (byte)(fileData[i] ^ mask[maskIndex]);
            maskIndex++;
            maskIndex = maskIndex % 4;
        }
    }
}
