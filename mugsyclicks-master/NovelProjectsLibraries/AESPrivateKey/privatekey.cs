using System;
using System.Collections.Generic;
using System.Text;

namespace NovelProjects.AESPrivateKey
{
  public class PrivateKey
  {
    byte[] Key;
    byte[] Vector;

    public PrivateKey()
    {
      this.Key = new byte[] { 48,174,117,232,149,157,80,83,84,161,204,160,110,20,57,7,80,212,115,176,87,138,19,159,52,37,74,217,222,154,170,2 };
      this.Vector = new byte[] { 194, 160, 172, 106, 254, 224, 155, 41, 229, 205, 173, 126, 127, 2, 62, 195 };
    }

    public byte[] GetKey()
    {
      return this.Key;
    }

    public byte[] GetVector()
    {
      return this.Vector;
    }
  }
}
