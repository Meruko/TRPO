using CatLib.Models;
using CatLib.Rest;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CatLib
{
    /// <summary>
    /// Логика взаимодействия для ReservationsPage.xaml
    /// </summary>
    public partial class ReservationsPage : Page
    {
        public ReservationsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Функция поиска и добавления брони в список найденных по её номеру
        /// </summary>
        /// <param name="search">Строк поиска (номер брони в виде строки)</param>
        private void SearchReservation(string search)
        {
            search = search.Trim();

            try
            {
                long id = long.Parse(search);

                SetReservation(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска!", "CatLib", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработка события нажатия на кнопку поиска брони.
        /// Получение значения из строки поиска и вызов функции установки брони
        /// </summary>
        /// <param name="sender">Элемент управления, который вызвал событие</param>
        /// <param name="e">Параметры события</param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchReservation(tbSearch.Text);
        }

        /// <summary>
        /// Обработка события нажатия кнопки Enter при нахождении на поле поиска.
        /// Получение значения из строки поиска и вызов функции установки брони
        /// </summary>
        /// <param name="sender">Элемент управления, который вызвал событие</param>
        /// <param name="e">Параметры события</param>
        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            SearchReservation(tbSearch.Text);
        }

        /// <summary>
        /// Функция отображения или скрытия элемента загрузки
        /// </summary>
        /// <param name="isLoading">True - отображать загрузку. False - отображать окно с результатом</param>
        private void DisplayLoading(bool isLoading)
        {
            Dispatcher.Invoke(() =>
            {
                if (isLoading)
                {
                    grdMain.Visibility = Visibility.Hidden;
                    grdLoading.Visibility = Visibility.Visible;
                }
                else
                {
                    grdMain.Visibility = Visibility.Visible;
                    grdLoading.Visibility = Visibility.Hidden;
                }
            });
        }

        /// <summary>
        /// Получение брони по её номеру и её отображение в списке найденных броней
        /// </summary>
        /// <param name="id">Номер брони</param>
        private void SetReservation(long id)
        {
            DisplayLoading(true);

            Task.Run(() =>
            {
                RestRequest request = new RestRequest("Reservations/withBook/{id}", Method.Get);
                request.AddUrlSegment<long>("id", id);

                try
                {
                    ReservationWithBook reservation = SingleRest.GetContext().ExecuteRequestAsync<ReservationWithBook>(request).GetAwaiter().GetResult().Data;

                    Dispatcher.Invoke(() =>
                    {
                        if (reservation != null)
                        {
                            lvReservtaions.ItemsSource = new List<ReservationWithBook> { reservation };
                        }
                        else
                        {
                            MessageBox.Show($"Бронь не найдена!", "CatLib", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении брони: {ex.Message}!", "CatLib", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DisplayLoading(false);
                }
            });
        }

        /// <summary>
        /// Обработка события загрузки кнопок подтверждения или отказа брони для их отображения
        /// </summary>
        /// <param name="sender">Элемент управления, который вызвал событие</param>
        /// <param name="e">Параметры события</param>
        private void ButtonReservation_Loaded(object sender, RoutedEventArgs e)
        {
            Button thisButton = (Button)sender;
            thisButton.Visibility = ((ReservationWithBook)thisButton.DataContext).Reservation.Status == "Активна" ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Функция подтверждения/отмены брони
        /// </summary>
        /// <param name="confirm">True - подтверждение брони. False - отмена брони</param>
        /// <param name="id">ID брони</param>
        private void ReservationDecision(bool confirm, long id)
        {
            RestRequest request = confirm ? new RestRequest("Reservations/confirm/{id}", Method.Put) : new RestRequest("Reservations/cancel/{id}", Method.Put);

            DisplayLoading(true);

            Task.Run(() =>
            {
                request.AddUrlSegment<long>("id", id);

                try
                {
                    SingleRest.GetContext().ExecuteRequestAsync<ReservationWithBook>(request).GetAwaiter().GetResult();

                    SetReservation(id);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "CatLib", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DisplayLoading(false);
                }
            });
        }

        /// <summary>
        /// Обработка события нажатия кнопки отказа брони
        /// </summary>
        /// <param name="sender">Элемент управления, который вызвал событие</param>
        /// <param name="e">Параметры события</param>
        private void ButtonCancelReservation_Click(object sender, RoutedEventArgs e)
        {
            Button thisButton = (Button)sender;
            long id = ((ReservationWithBook)thisButton.DataContext).Reservation.Id.Value;

            switch (MessageBox.Show("Вы точно хотите отменить выбранную бронь?", "CatLib", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    {
                        ReservationDecision(false, id);
                        break;
                    }
            }
        }

        private void ButtonConfirmReservation_Click(object sender, RoutedEventArgs e)
        {
            Button thisButton = (Button)sender;
            long id = ((ReservationWithBook)thisButton.DataContext).Reservation.Id.Value;

            switch (MessageBox.Show("Вы точно хотите подтвердить выбранную бронь?", "CatLib", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    {
                        ReservationDecision(true, id);
                        break;
                    }
            }
        }

        /// <summary>
        /// Обработка события загрузки изображения у карточки брони
        /// </summary>
        /// <param name="sender">Элемент управления, который вызвал событие</param>
        /// <param name="e">Параметры события</param>
        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    Border border = (Border)sender;
                    Book currentBook = ((ReservationWithBook)border.DataContext).Book;

                    if (border == null || currentBook == null)
                        return;

                    try
                    {
                        border.Background = new ImageBrush(SingleRest.GetContext().DownloadImageAsync(currentBook.Image).GetAwaiter().GetResult());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                    }
                });
            });
        }
    }
}