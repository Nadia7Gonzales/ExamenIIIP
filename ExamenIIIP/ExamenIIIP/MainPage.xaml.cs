using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamenIIIP.Controllers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;
using Xamarin.Essentials;
using Plugin.AudioRecorder;
using Acr.UserDialogs;
using ExamenIIIP.Views;
using System.Collections;

namespace ExamenIIIP
{
    public partial class MainPage : ContentPage
    {
        byte[] Image;
        MediaFile file;
        private AudioRecorderService audioRecorderService = new AudioRecorderService()
        {
            StopRecordingOnSilence = false,
            StopRecordingAfterTimeout = false
        };

        private AudioPlayer audioPlayer = new AudioPlayer();

        private bool reproducir = false;
        MediaFile FileFoto = null;
        public MainPage()
        {
            InitializeComponent();
           
        

        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            bool response = await Application.Current.MainPage.DisplayAlert("Advertencia", "Por favor seleccione", "Camara", "Galeria");

            if (response)
                GetImageFromCamera();
            else
                TakeImageFromGallery();
        }
        private async void GetImageFromCamera()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status == PermissionStatus.Granted)
            {
                try
                {
                    file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {

                        PhotoSize = PhotoSize.Medium
                    });
                    if (file == null)
                    {
                        return;
                    }
                    imgFoto.Source = ImageSource.FromStream(() =>
                    {
                        return file.GetStream();
                    });
                }
                catch (Exception ex)
                {

                }
            }
        }
        private async void AddNewNota()
        {
            String DescriptionNota = DescripcionNotaText.Text;
            DateTime date = (FechaNotaText.Date);

            string str = string.Format("dd-MM-yyyy");
            string ab = date.ToString(str);

            String FechaNota = ab;



            string PhotoNota;
            Byte[] AudioNota = ConvertAudioToByteArray();
            

                if (file == null || string.IsNullOrEmpty(DescriptionNota) || string.IsNullOrEmpty(FechaNota) || !reproducir)
                {
                    await Application.Current.MainPage.DisplayAlert("Alerta", "Necesita llenar todos los campos del formulario.", "OK");
                }
                else
                {

                PhotoNota = await NotasController.SaveImage(file.GetStream(), Path.GetFileName(file.Path));
                bool res = await NotasController.AddNota(DescriptionNota, FechaNota, PhotoNota, AudioNota);

                    if (res)
                    {
                        await Application.Current.MainPage.DisplayAlert("Muy bien", "Nota agregada satisfactoriamente.", "OK");

                        imgFoto.Source = "https://cdn-icons-png.flaticon.com/512/4001/4001475.png";
                        DescripcionNotaText.Text = "";
                        FechaNotaText.Date = DateTime.Now;
                        
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Alerta", "No se pudo agregar la nota.", "OK");
                    }
                }
            }
            
        

        private void Button_Clicked(object sender, EventArgs e)
        {
            AddNewNota();
        }
        private async void btnGrabar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Microphone>();
                var status2 = await Permissions.RequestAsync<Permissions.StorageRead>();
                var status3 = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (status != PermissionStatus.Granted & status2 != PermissionStatus.Granted & status3 != PermissionStatus.Granted)
                {
                    return; // si no tiene los permisos no avanza
                }

                if (audioRecorderService.IsRecording)
                {
                    await audioRecorderService.StopRecording();


                    audioPlayer.Play(audioRecorderService.GetAudioFilePath());

              

                    btnGrabar.Text = "Grabar";

                    reproducir = true;
                }
                else
                {
                    await audioRecorderService.StartRecording();


                    

                    btnGrabar.Text = "Parar";

             
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alerta", ex.Message, "OK");
            }

        }
        public byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        private Byte[] ConvertAudioToByteArray()
        {
            Stream audioFile = audioRecorderService.GetAudioFileStream();

            

            Byte[] bytes = ReadFully(audioFile);
            return bytes;
        }

        private async void TakeImageFromGallery()
        {
            await CrossMedia.Current.Initialize();
            try
            {
                file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    PhotoSize = PhotoSize.Medium
                });
                if (file == null)
                {
                    return;
                }
                imgFoto.Source = ImageSource.FromStream(() =>
                {
                    return file.GetStream();
                });
            }
            catch (Exception ex)
            {

            }
        }

        private async void Message(string title, string message)
        {
            await DisplayAlert(title, message, "OK");
        }
        private async void btnList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new Notas_List());
        }

    }
}
