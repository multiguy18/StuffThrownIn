using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;

namespace LumpReader
{
    public partial class MainWindow : Window
    {
        WriteableBitmap bitmap;
        List<Color> palette = new List<Color>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SetPalette(string name)
        {
            palette.Clear();

            FileStream palStream = new FileStream(name, FileMode.Open);
            BinaryReader palReader = new BinaryReader(palStream);

            try
            {
                for (int i = 0; i < 256; i++)
                {
                    palette.Add(Color.FromRgb(palReader.ReadByte(), palReader.ReadByte(), palReader.ReadByte()));
                }
            }
            catch
            {
                MessageBox.Show("The palette file could not be opened or is damaged.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                palette.Clear();
            }
            finally
            {
                palStream.Close();
            }
        }

        private void InitGraphics(IList<Color> palette, bool emulateAh13Aspect)
        {
            double dpiMulY = 1;

            if (emulateAh13Aspect == true)
            {
                dpiMulY *= 0.8;
            }

            RenderOptions.SetBitmapScalingMode(imgElem, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(imgElem, EdgeMode.Aliased);

            bitmap = new WriteableBitmap(300, 300, 96, 96 * dpiMulY, PixelFormats.Indexed8, new BitmapPalette(palette) ?? null);

            imgElem.Source = bitmap;
        }

        private void DrawGraphic(string name)
        {

            FileStream graphicsStream = new FileStream(name, FileMode.Open);
            BinaryReader grapicsReader = new BinaryReader(graphicsStream);

            try
            {
                bitmap.Lock();

                ushort width = grapicsReader.ReadUInt16();
                ushort height = grapicsReader.ReadUInt16();
                short leftoffset = grapicsReader.ReadInt16();
                short rightoffset = grapicsReader.ReadInt16();
                uint[] columnofs = new uint[width];
                for (int i = 0; i < width; i++)
                {
                    columnofs[i] = grapicsReader.ReadUInt32();
                }

                for (int x = 0; x < columnofs.Length; x++)
                {
                    uint postofs = columnofs[x];

                    graphicsStream.Seek(postofs, SeekOrigin.Begin);

                    byte topdelta = grapicsReader.ReadByte();
                    byte length = grapicsReader.ReadByte();
                    grapicsReader.ReadByte();
                    byte[] data = grapicsReader.ReadBytes(length);

                    for (int y = 0; y < length; y++)
                    {
                        unsafe
                        {
                            IntPtr bpPtr = bitmap.BackBuffer;

                            byte colorInfo = data[y];

                            int ypos = y + topdelta;


                            if (ypos < bitmap.PixelHeight && x < bitmap.PixelWidth)
                            {
                                bpPtr += ypos * bitmap.BackBufferStride;
                                bpPtr += x * 1;

                                *(byte*)bpPtr = colorInfo;
                            }
                        }
                    }
                }


                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            catch
            {
                MessageBox.Show("The graphic file could not be opened or is damaged.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                graphicsStream.Close();
                bitmap.Unlock();
            }
        }

        private void DrawFlat(string name)
        {
            FileStream flatStream = new FileStream(name, FileMode.Open);
            BinaryReader flatReader = new BinaryReader(flatStream);

            try
            {
                bitmap.Lock();


                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        unsafe
                        {
                            IntPtr bpPtr = bitmap.BackBuffer;

                            byte colorInfo = flatReader.ReadByte();

                            bpPtr += y * bitmap.BackBufferStride;
                            bpPtr += x * 1;

                            *(byte*)bpPtr = colorInfo;
                        }
                    }
                }

                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            catch
            {
                MessageBox.Show("The flat file could not be opened or is damaged.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                flatStream.Close();
                bitmap.Unlock();
            }
        }

        private void Clear()
        {
            try
            {
                bitmap.Lock();

                for (int y = 0; y < bitmap.PixelHeight; y++)
                {
                    for (int x = 0; x < bitmap.PixelWidth; x++)
                    {
                        unsafe
                        {
                            IntPtr bpPtr = bitmap.BackBuffer;

                            byte colorInfo = 0;

                            bpPtr += y * bitmap.BackBufferStride;
                            bpPtr += x * 1;

                            *(byte*)bpPtr = colorInfo;
                        }
                    }
                }

                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.RestoreDirectory = true;
            fileDialog.Filter = "All Pictures (*.lmp;*.raw)|*.lmp;*.raw|Grapics (*.lmp)|*.lmp|Flats (*.raw)|*.raw|All files (*.*)|*.*";
            if (fileDialog.ShowDialog() ?? false)
            {
                string filepath = fileDialog.FileName;

                if (palette.Any())
                {
                    switch (filepath.Split('.').Last())
                    {
                        case "lmp":
                            InitGraphics(palette, true);
                            DrawGraphic(filepath);
                            break;
                        case "raw":
                            InitGraphics(palette, false);
                            DrawFlat(filepath);
                            break;
                        default:
                            MessageBox.Show("File type not supported.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("No palette was loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItem_Clear(object sender, RoutedEventArgs e)
        {
            if (bitmap != null)
            {
                Clear();
            }
        }

        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_OpenPalette(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.RestoreDirectory = true;
            fileDialog.Filter = "Palette (*.pal)|*.pal|All files (*.*)|*.*";
            if (fileDialog.ShowDialog() ?? false)
            {
                string filepath = fileDialog.FileName;

                if (filepath.Split('.').Last() == "pal")
                {
                    SetPalette(filepath);
                }
                else
                {
                    MessageBox.Show("File type not supported.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

