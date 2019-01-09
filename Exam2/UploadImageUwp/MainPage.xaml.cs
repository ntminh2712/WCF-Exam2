using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UploadImageUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Select_Photo(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,

                ViewMode = PickerViewMode.Thumbnail
            };

            fileOpenPicker.FileTypeFilter.Clear();
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".bmp");

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                IRandomAccessStream fileStream =
                await file.OpenAsync(FileAccessMode.Read);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);

                IBuffer buffer = await FileIO.ReadBufferAsync(file);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=avatarupload;AccountKey=6xmfe3sqMzN0ctDxrNYui5Oailjp2J3pVmri5v4PJMJXy5FGmn3ZeUkiR1W7ontVCaovwWYbHRgjwoOEu/8abA==;EndpointSuffix=core.windows.net");
                CloudBlobContainer cloudBlobContainer = null;

                string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");


                try
                {
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    cloudBlobContainer = cloudBlobClient.GetContainerReference("avatarupload");
                    await cloudBlobContainer.CreateIfNotExistsAsync();

                    // Set the permissions so the blobs are public. 
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await cloudBlobContainer.SetPermissionsAsync(permissions);

                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(file.Name.ToString());
                    await cloudBlockBlob.UploadFromFileAsync(file);

                    BlobContinuationToken blobContinuationToken = null;
                    try
                    {
                        var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                        blobContinuationToken = results.ContinuationToken;
                        foreach (IListBlobItem item in results.Results)
                        {
                            Debug.WriteLine(item.Uri);
                            ImageUrl.Text = item.Uri.ToString();
                            YourAvatar.ProfilePicture = bitmapImage;
                        }
                    }
                    catch (Exception err)
                    {
                        Debug.WriteLine(err);
                    }
                }
                catch (StorageException ex)
                {
                    Debug.WriteLine("Error returned from the service: {0}", ex.Message);
                }
            }
        }
    }
}
