namespace FluentHyperV.Cloudinit.Models;

/// <summary>
/// Cloud image configuration
/// </summary>
public class ImageConfiguration
{
    public string ImageVersion { get; set; } = "22.04";

    public string ImageRelease { get; set; } = "release";

    public string ImageBaseUrl { get; set; } =
        "https://mirror.nju.edu.cn/ubuntu-cloud-images/releases";

    public bool BaseImageCheckForUpdate { get; set; } = true;
}
