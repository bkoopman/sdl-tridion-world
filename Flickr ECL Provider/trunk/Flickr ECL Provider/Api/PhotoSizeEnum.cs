using System.ComponentModel;

namespace Example.EclProvider.Api
{
    /// <summary>
    /// Publicly available photo sizes.
    /// </summary>
    public enum PhotoSizeEnum
    {
        /// <summary>
        /// width: 75, height: 75
        /// </summary>
        [Description("_s")]
        Square,

        /// <summary>
        /// width: 150, height: 150
        /// </summary>
        [Description("_q")]        
        LargeSquare,

        /// <summary>
        /// width: 100, height: 75
        /// </summary>
        [Description("_t")]
        Thumbnail,
        
        /// <summary>
        /// width: 240, height: 180
        /// </summary>
        [Description("_m")]
        Small,
        
        /// <summary>
        /// width: 320, height: 240
        /// </summary>
        [Description("_n")]
        Qvga,
        
        /// <summary>
        /// width: 500, height: 375
        /// </summary>
        [Description("")]
        Medium,
        
        /// <summary>
        /// width: 640, height: 480
        /// </summary>
        [Description("_z")]
        Vga,
        
        /// <summary>
        /// width: 800, height: 600
        /// </summary>
        [Description("_c")]
        Svga,
        
        /// <summary>
        /// width: 1024, height: 768
        /// </summary>
        [Description("_b")]
        Large
    }
}
