using NUnit.Framework;
using UnityEditor;



public class EditMode
{
    [Test]
    public void SumaBasica()
    {
        int num1 = 1;
        int num2 = 5;

        int resultado = num1 + num2;

        Assert.AreEqual(6, resultado);
    }
}
