// Decompiled with JetBrains decompiler
// Type: CyberInvasion.Properties.Resources
// Assembly: CyberInvasion, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 26DD4A83-2C31-4F96-A7DD-BF680ED03A8A
// Assembly location: C:\Users\Googlelai\Desktop\CyberInvasion.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CyberInvasion.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (CyberInvasion.Properties.Resources.resourceMan == null)
          CyberInvasion.Properties.Resources.resourceMan = new ResourceManager("CyberInvasion.Properties.Resources", typeof (CyberInvasion.Properties.Resources).Assembly);
        return CyberInvasion.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => CyberInvasion.Properties.Resources.resourceCulture;
      set => CyberInvasion.Properties.Resources.resourceCulture = value;
    }
  }
}
