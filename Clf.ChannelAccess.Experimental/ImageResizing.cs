//
// ImageResizing.cs
//

namespace Clf.ChannelAccess.Experimental
{

  //
  // Convenient scratchpad for experiments with ImageViewer in Clf.Blazor.Components
  //

  //
  // To do resizing in JS ...
  //
  // https://developer.mozilla.org/en-US/docs/Web/API/ImageBitmap
  // The ImageBitmap interface represents a bitmap image which can be drawn to a <canvas> without undue latency.
  // It can be created from a variety of source objects using the createImageBitmap() factory method.
  //
  // https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/drawImage
  // The CanvasRenderingContext2D.drawImage() method of the Canvas 2D API provides different ways to draw an image onto the canvas.
  //   void ctx.drawImage(image, dx, dy);
  //   void ctx.drawImage(image, dx, dy, dWidth, dHeight);
  //   void ctx.drawImage(image, sx, sy, sWidth, sHeight, dx, dy, dWidth, dHeight);
  //  The 'image' can be any canvas image source (CanvasImageSource), specifically, a CSSImageValue,
  //  an HTMLImageElement, an SVGImageElement, an HTMLVideoElement, an HTMLCanvasElement, an ImageBitmap, or an OffscreenCanvas.
  //
  // https://developer.mozilla.org/en-US/docs/Web/API/createImageBitmap
  // The createImageBitmap() method creates a bitmap from a given source,
  // which can be an <img>, SVG <image>, <video>, <canvas>, HTMLImageElement, SVGImageElement,
  // HTMLVideoElement, HTMLCanvasElement, Blob, ImageData, ImageBitmap, or OffscreenCanvas object.
  //
  // https://developer.mozilla.org/en-US/docs/Web/API/ImageData
  // The ImageData interface represents the underlying pixel data of an area of a <canvas> element.
  // It can also be used to set a part of the canvas by using putImageData().
  //
  // -----------------------
  //
  // function CanvasPutImageData ( canvas, grayScaleData, displayWidth, displayHeight )
  // {
  //   const ctx = canvas.getContext('2d') ;
  //   const imageData = ctx.createImageData(displayWidth,displayHeight) ;
  //   var iTarget = 0 ;
  //   for ( var i = 0 ; i < grayScaleData.length ; i++)
  //   {
  //     imageData.data[iTarget + 0] = grayScaleData[i] ;
  //     imageData.data[iTarget + 1] = grayScaleData[i] ;
  //     imageData.data[iTarget + 2] = grayScaleData[i] ;
  //     imageData.data[iTarget + 3] = 255 ;
  //     iTarget += 4 ;
  //   }
  //   // Draw image data to the canvas
  //   ctx.putImageData(imageData, 0, 0, 0, 0, displayWidth, displayHeight);
  // }
  //

  //
  // Can also use functions in .Net Core :
  // https://www.hanselman.com/blog/how-do-you-use-systemdrawing-in-net-core
  // Create an Image, write to Stream as a PNG or whatever, can then use as 'img' source.
  //

  public static class ImageResizing
  {

    public static byte[] CreateInterpolatedGreyScaleImage ( 
      byte[] originalImage, 
      int    originalWidthX, 
      int    originalHeightY,
      int    interpolatedWidthX, 
      int    interpolatedHeightY
    ) {
      byte[] interpolatedImage = new byte[
        interpolatedWidthX 
      * interpolatedHeightY
      ] ;
      double xFactor = ( (double) originalWidthX  ) / interpolatedWidthX ;
      double yFactor = ( (double) originalHeightY ) / interpolatedHeightY ;
      for ( int yInterpolated = 0 ; yInterpolated < interpolatedHeightY ; yInterpolated++ ) 
      {
        int originalIndexY = (int) ( yInterpolated * yFactor ) ;
        for ( int xInterpolated = 0 ; xInterpolated < interpolatedWidthX ; xInterpolated++ )
        {
          int originalIndexX = (int) ( xInterpolated * xFactor ) ;
          byte originalPixel = GetOriginalPixel(
            originalIndexX,
            originalIndexY
          ) ;
          WriteInterpolatedPixel(
            xInterpolated,
            yInterpolated,
            originalPixel
          ) ;
        }
      }
      return interpolatedImage ;
      // Local functions - turns out that these don't get inlined,
      // even in a Release build, so the overhead of the function call
      // probably isn't worth the readability improvement ...
      byte GetOriginalPixel ( int xOriginal, int yOriginal )
      {
        int index = (
          xOriginal
        + yOriginal * originalWidthX
        ) ;
        // if ( index >= originalImage.Length )
        // {
        //   index = originalImage.Length - 1;
        // }
        return originalImage[index] ;
      }
      void WriteInterpolatedPixel ( int xInterpolated, int yInterpolated, byte pixel )
      {
        int index = (
          xInterpolated
        + yInterpolated * interpolatedWidthX
        ) ;
        // if ( index >= interpolatedImage.Length )
        // {
        //   index = interpolatedImage.Length - 1;
        // }
        interpolatedImage[index] = pixel ;
      }

    }

    // https://tech-algorithm.com/articles/nearest-neighbor-image-scaling/
    // https://github.com/mdavid/aforge.net/blob/master/Sources/Imaging/Filters/Transform/ResizeNearestNeighbor.cs

    private static byte[] CreateNearestNeighbourInterpolatedGreyScaleImage (
      byte[] originalImage,
      int    originalWidthX,
      int    originalHeightY,
      int    interpolatedWidthX,
      int    interpolatedHeightY
    ) {
      byte[] interpolatedImage = new byte[
        interpolatedWidthX
      * interpolatedHeightY
      ] ;
      double xFactor = ( (double) originalWidthX)  / interpolatedWidthX  ;
      double yFactor = ( (double) originalHeightY) / interpolatedHeightY ;
      for ( int yInterpolated = 0 ; yInterpolated < interpolatedHeightY ; yInterpolated++ )
      {
        int originalIndexY = (int) ( yInterpolated * yFactor ) ;
        for ( int xInterpolated = 0 ; xInterpolated < interpolatedWidthX ; xInterpolated++ )
        {
          int originalIndexX = (int) ( xInterpolated * xFactor ) ;
          // GetOriginalPixel
          byte originalPixel = originalImage[
            originalIndexX 
          + originalIndexY * originalWidthX
          ] ;
          // WriteInterpolatedPixel
          interpolatedImage[
            xInterpolated
          + yInterpolated * interpolatedWidthX
          ] = originalPixel ;
        }
      }
      return interpolatedImage ;
    }

  }

}

