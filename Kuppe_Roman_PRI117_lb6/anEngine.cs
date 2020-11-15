using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.IO;

using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;

namespace Kuppe_Roman_PRI117_lb6 {

    public class anBrush {
        public Bitmap myBrush;

        // стандартная (квадратная) кисть, с указанием масштаба
        // и флагом закраски узлов

        public anBrush(int Value, bool Special) {
            if (!Special) {
                myBrush = new Bitmap(Value, Value);

                for (int ax = 0; ax < Value; ax++)
                    for (int bx = 0; bx < Value; bx++)
                        myBrush.SetPixel(0, 0, Color.Black);

                // не является ластиком
                IsErase = false;
            } else {
                // здесь мы будем размещать предустановленные кисти
                // созданная нами ранее кисть в виде перекрестия двух линий будет китсью по умолчанию
                // на тот случай, если задан не описанный номер кисти

                switch (Value) {
                    // специальная кисть по умолчанию
                    default: {
                        myBrush = new Bitmap(5, 5);

                        for (int ax = 0; ax < 5; ax++)
                            for (int bx = 0; bx < 5; bx++)
                                myBrush.SetPixel(ax, bx, Color.Red);

                        myBrush.SetPixel(0, 2, Color.Black);
                        myBrush.SetPixel(1, 2, Color.Black);

                        myBrush.SetPixel(2, 0, Color.Black);
                        myBrush.SetPixel(2, 1, Color.Black);
                        myBrush.SetPixel(2, 2, Color.Black);
                        myBrush.SetPixel(2, 3, Color.Black);
                        myBrush.SetPixel(2, 4, Color.Black);

                        myBrush.SetPixel(3, 2, Color.Black);
                        myBrush.SetPixel(4, 2, Color.Black);

                        // не является ластиком
                        IsErase = false;

                        break;
                    }
                    case 1: // стерка
                    {
                        // создается так же, как и обычная кисть,
                        // но имеет флаг IsErase равный true
                        myBrush = new Bitmap(5, 5);

                        for (int ax = 0; ax < Value; ax++)
                            for (int bx = 0; bx < Value; bx++)
                                myBrush.SetPixel(0, 0, Color.Black);

                        // является ластиком
                        IsErase = true;
                        break;
                    }
                }
            }
        }

        // второй конструктор будет позволять загружать кисть из стороннего файла
        public anBrush(string FromFile) {
            string path = Directory.GetCurrentDirectory();
            path += "\\" + FromFile;

            myBrush = new Bitmap(path);
        }

        // флаг, сигнализирующий о том, что установленная кисть является ластиком
        private bool IsErase = false;

        // функция, которая будет использоваться для получения информации
        // о том, является ли данная кисть ластиком
        public bool IsBrushErase() {
            return IsErase;
        }
    }

    public class anLayer {
        // размеры экранной области
        public int Width, Heigth;

        // массив , представляющий область рисунка (координаты пикселя и его цвет)
        private int[,,] DrawPlace;

        // флаг видимости слоя: true - видимый, false - невидимый
        private bool isVisible;

        // текущий установленный цвет
        private Color ActiveColor;

        // функция получения массива пикселей
        public int[,,] GetDrawingPlace() {
            return DrawPlace;
        }

        // конструктор класса, в качестве входных параметров
        // мы получаем размеры изображения, чтобы создать в памяти массив,
        // который будет хранить растровые данные для этого слоя

        public anLayer(int s_W, int s_H) {
            // запоминаем значения размеров рисунка
            Width = s_W;
            Heigth = s_H;

            // создаем в памяти массив, соотвествующий размерам рисунка
            // каждая точка на полскости массива будет иметь 3 составляющие цвета
            // + 4 ячейка - индикатор того, что данный пиксель пуст (или полность прозрачен)
            DrawPlace = new int[Width, Heigth, 4];

            // проходим по всей плоскости и устанавливаем всем точкам
            // индикатор прозрачности
            for (int ax = 0; ax < Width; ax++) {
                for (int bx = 0; bx < Heigth; bx++) {
                    // флаг прозачности точки в координатах ax,bx.
                    DrawPlace[ax, bx, 3] = 1;
                }
            }

            // устанавливаем флаг видимости слоя (по умолчанию создаваемый слой всегда видимый)
            isVisible = true;

            // текущим активным цветом устанавливаем черный
            // в следующей главе мы реализуем функции установки цветов из оболочки
            ActiveColor = Color.Black;
        }

        // функция установки режима видимости слоя
        public void SetVisibility(bool visiblityState) {
            isVisible = visiblityState;
        }

        // функция получения текущего состояния видимости слоя
        public bool GetVisibility() {
            return isVisible;
        }

        // функция рисования
        // получает в качестве параметров кисть для рисования и координаты,
        // где сейчас необходимо перерисовать пиксели заданной кистью
        public void Draw(anBrush BR, int x, int y) {
            // определяем начальную позицию рисования

            int real_pos_draw_start_x = x - BR.myBrush.Width / 2;
            int real_pos_draw_start_y = y - BR.myBrush.Height / 2;

            // корректируем ее для невыхода за границы массива
            // проверка на отрицательные значения (граница "слова")
            if (real_pos_draw_start_x < 0)
                real_pos_draw_start_x = 0;

            if (real_pos_draw_start_y < 0)
                real_pos_draw_start_y = 0;

            // проверки на выход за границу "справа"
            int boundary_x = real_pos_draw_start_x + BR.myBrush.Width;
            int boundary_y = real_pos_draw_start_y + BR.myBrush.Height;


            if (boundary_x > Width)
                boundary_x = Width;

            if (boundary_y > Heigth)
                boundary_y = Heigth;

            // счетчик пройденных строк и столбцов массива, прдстваляющего собой маску кисти
            int count_x = 0, count_y = 0;

            // цикл по области с учетом смещения кисти и коорекции для невыхода за границы массива
            for (int ax = real_pos_draw_start_x; ax < boundary_x; ax++, count_x++) {
                count_y = 0;
                for (int bx = real_pos_draw_start_y; bx < boundary_y; bx++, count_y++) {
                    // проверяем, не является ли данная кисть ластиком
                    if (BR.IsBrushErase()) {
                        // данная кисть - ластик
                        // помечаем данный пиксель как незакрашенный
                        // получаем текущий цвет пикселя маски
                        Color ret = BR.myBrush.GetPixel(count_x, count_y);

                        // цвет не красный
                        if (!(ret.R == 255 && ret.G == 0 && ret.B == 0)) {
                            // заполняем данный пиксель активным цветом из маски
                            DrawPlace[ax, bx, 3] = 1;
                        }
                    } else {
                        // получаем текущий цвет пикселя маски
                        Color ret = BR.myBrush.GetPixel(count_x, count_y);

                        // цвет не красный
                        if (!(ret.R == 255 && ret.G == 0 && ret.B == 0)) {
                            // заполняем данный пиксель активным цветом из маски

                            DrawPlace[ax, bx, 0] = ActiveColor.R;
                            DrawPlace[ax, bx, 1] = ActiveColor.G;
                            DrawPlace[ax, bx, 2] = ActiveColor.B;
                            DrawPlace[ax, bx, 3] = 0;
                        }
                    }
                }
            }
        }

        // функция визуализации слоя
        // функция визуализации слоя
        public void RenderImage(bool FromList) {
            if (FromList) // указана визуализация из дисплейного списка, следовательно данный слой не активен
            {
                // вызываем дисплейный список
                Gl.glCallList(ListNom);
            } else // данный слой активен и визуализацию необходимо делать на ходу
              {
                // счетчик номеров элементов, которые должны участвовать в визуализации
                int count = 0;

                // проходим по всем точкам рисунка
                for (int ax = 0; ax < Width; ax++) {
                    for (int bx = 0; bx < Heigth; bx++) {
                        // если точка в координатах ax,bx не помечена флагом "прозрачная"
                        if (DrawPlace[ax, bx, 3] != 1) {
                            // не лучшие способ, но так мы подсчитаем количество действительно значимых точек слоя,
                            // которые должны быть визуализированны
                            count++;
                        }
                    }
                }

                // данный массив будет заполнен, а затем передан для быстрой отрисовки геометрии (в нашем случае - точек)
                // колич. точек * 2 (для хранения координат x и y каждой точки, которая будет отрисована)
                int[] arr_date_vertex = new int[count * 2];

                // данный массив будет содержать значения цветов для всех отрисовываемых точек
                // колич. точек * 3 (для хранения координат R G B значений цветов каждой точки, которая будет отрисована)
                float[] arr_date_colors = new float[count * 3];

                // счетчик элементов для создания массивов, которые будут переданы в реализацию OpenGL c
                // помощью функции glDrawArrays
                int now_element = 0;

                // теперь, когда мы выделили массив необходимого размера,
                // заполним его необходимыми значениями
                for (int ax = 0; ax < Width; ax++) {
                    for (int bx = 0; bx < Heigth; bx++) {
                        // если точка с координатами ax,bx не помечена флагом "прозрачная"
                        // если данная точка НЕ помечена флагом, сигнализирующим о том, что она не должна быть визуализированна
                        if (DrawPlace[ax, bx, 3] != 1) {
                            // заносим координаты точки (ax , bx ) в массив, который будет передан для визуализации
                            arr_date_vertex[now_element * 2] = ax;
                            arr_date_vertex[now_element * 2 + 1] = bx;

                            // заносим значения составляющих цвета, сразу перенося их в формат float
                            arr_date_colors[now_element * 3] = (float)DrawPlace[ax, bx, 0] / 255.0f;
                            arr_date_colors[now_element * 3 + 1] = (float)DrawPlace[ax, bx, 1] / 255.0f;
                            arr_date_colors[now_element * 3 + 2] = (float)DrawPlace[ax, bx, 2] / 255.0f;

                            // подсчет добавленных элементов в массивы
                            now_element++;
                        }
                    }
                }

                // теперь, когда массивы с геометрическими данными и данными о цветах подготовлены,
                // включаем функцию использования массивов вершин и цветов

                Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
                Gl.glEnableClientState(Gl.GL_COLOR_ARRAY);

                // передаем массивы вершин и цветов, указывая количество элементов массива, приходящихся
                // на один визуализируемый элемент (в случае точек - 2 координаты: х и у, в случае цветов - 3 составляющие цвета)

                Gl.glColorPointer(3, Gl.GL_FLOAT, 0, arr_date_colors);
                Gl.glVertexPointer(2, Gl.GL_INT, 0, arr_date_vertex);

                // вызываем функцию glDrawArrays, которая позволит нам визуализировать наши массивы, передав их целиком,
                // а не передавая в цикле каждую точку
                Gl.glDrawArrays(Gl.GL_POINTS, 0, count);

                // деактивируем режим использования массивов геометрии и цветов
                Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
                Gl.glDisableClientState(Gl.GL_COLOR_ARRAY);
            }
        }

        // установка текущего цвета для рисования в слое
        public void SetColor(Color NewColor) {
            ActiveColor = NewColor;
        }

        // получение текущего активного цвета
        public Color GetColor() {
            // возвращаем цвет
            return ActiveColor;
        }

        // функции удаления слоя
        public void ClearList() {
            // проверяем факт существования дисплейного списка с номером, хранимым в ListNom
            if (Gl.glIsList(ListNom) == Gl.GL_TRUE) {
                // удаляем его в случае существования
                Gl.glDeleteLists(ListNom, 1);
            }
        }
        int  ListNom = 0;
        public void CreateNewList() {
            // проверяем факт существования дисплейного списка с номером, хранимым в ListNom
            if (Gl.glIsList(ListNom) == Gl.GL_TRUE) {
                // удаляем его в случае существования
                Gl.glDeleteLists(ListNom, 1);
                // и генерируем новый номер
                ListNom = Gl.glGenLists(1);
            }

            // создаем дисплейный список
            Gl.glNewList(ListNom, Gl.GL_COMPILE);

            // вызывая обычную визуализацию (не из списка)
            RenderImage(false);

            // заверщаем создание дисплейного списка
            Gl.glEndList();
        }
    }

    class anEngine {
        // последний установленный цвет
        private Color LastColorInUse;

        // размеры изображения
        private int picture_size_x, picture_size_y;

        // положение полос прокрутки будет использовано в будующем
        private int scroll_x, scroll_y;

        // размер оконной части (объекта AnT)
        private int screen_width, screen_height;

        // номер активного слоя
        private int ActiveLayerNom;

        // массив слоев
        private ArrayList Layers = new ArrayList();

        // стандартная кисть
        private anBrush standartBrush;

        // конструктор класса
        public anEngine(int size_x, int size_y, int screen_w, int screen_h) {
            // создание кисти по умолчанию
            standartBrush = new anBrush(3, false);

            // при инициализации экземпляра класса сохраним настройки
            // размеров элементов и изображения в локальных переменных

            picture_size_x = size_x;
            picture_size_y = size_y;

            screen_width = screen_w;
            screen_height = screen_h;

            // полосы прокрутки у нас пока отсутствуют, поэтому просто обнулим значение переменных
            scroll_x = 0;
            scroll_y = 0;

            // добавим новый слой для работы, пока что он будет единственным
            Layers.Add(new anLayer(picture_size_x, picture_size_y));

            // номер активного слоя - 0
            ActiveLayerNom = 0;

            // и создадим стандартную кисть
            standartBrush = new anBrush(1, false);
            //standartBrush = new anBrush();
        }

        // функция установки стандартной кисти, предается только размер
        public void SetStandartBrush(int SizeB) {
            standartBrush = new anBrush(SizeB, false);
        }

        // функция установки специальной кисти
        public void SetSpecialBrush(int Nom) {
            standartBrush = new anBrush(Nom, true);
        }

        // установка кисти из файла
        public void SetBrushFromFile(string FileName) {
            standartBrush = new anBrush(FileName);
        }

        // функция установки активного цвета
        public void SetColor(Color NewColor) {
            ((anLayer)Layers[ActiveLayerNom]).SetColor(NewColor);
            LastColorInUse = NewColor;
        }


        // функция для установки номера активного слоя
        public void SetActiveLayerNom(int nom) {
            // текущий слой больше не будет активным, следовательно надо создать новый дисплейный список для его быстрой визуализации
            ((anLayer)Layers[ActiveLayerNom]).CreateNewList(); // новый активный слой получает установленный активный цвет для предыдущего активного слоя
            ((anLayer)Layers[nom]).SetColor(((anLayer)Layers[ActiveLayerNom]).GetColor());

            // установка номера активного слоя
            ActiveLayerNom = nom;
        }

        // установка видимости / невидимости слоя
        public void SetLayerVisibility(int nom, bool visible) {
            // вернемся к этой функции позже
        }

        // рисование текущей кистью
        public void Drawing(int x, int y) {
            // транслируем координаты, в которых проходит рисование, стандартной кистью
            ((anLayer)Layers[ActiveLayerNom]).Draw(standartBrush, x, y);
        }

        // визуализация
        public void SwapImage() {
            // вызываем функцию визуализации в нашем слое
            for (int ax = 0; ax < Layers.Count; ax++) {
                // если этот слой является активным в данный момент
                if (ax == ActiveLayerNom) {
                    // вызываем визуализацию данного слоя напрямую
                    ((anLayer)Layers[ax]).RenderImage(false);
                } else {
                    // вызываем визуализацию слоя из дисплейного списка
                    ((anLayer)Layers[ax]).RenderImage(true);
                }
            }
        }

        // функция добавления слоя
        public void AddLayer() {
            // добавляем слой в массив слоев ArrayList
            int AddingLayer = Layers.Add(new anLayer(picture_size_x, picture_size_y));
            // устанавливаем его активным
            //SetActiveLayerNom(AddingLayer);
        }

        // функция удаления слоев
        public void RemoveLayer(int nom) {
            // если номер корректен (в диапазоне добавленных в ArrayList)
            if (nom < Layers.Count && nom >= 0) {
                // делаем активным слой 0
                SetActiveLayerNom(0);
                // удаляем запись о слое
                Layers.RemoveAt(nom);

            }
        }

        // получение финального изображения
        public Bitmap GetFinalImage() {
            // заготовка результирующего изображения
            Bitmap resaultBitmap = new Bitmap(picture_size_x, picture_size_y); // данное решение также не является оптимальным по быстродействию, но при этом является самым простым способом решения задачи

            for (int ax = 0; ax < Layers.Count; ax++) {
                // получаем массив пикселей данного слоя
                int[,,] tmp_layer_data = ((anLayer)Layers[ax]).GetDrawingPlace();

                // пройдем двумя циклами по информации о пикселях данного слоя
                for (int a = 0; a < picture_size_x; a++) {
                    for (int b = 0; b < picture_size_y; b++) {
                        // если пиксель не помечен как "прозрачный"
                        if (tmp_layer_data[a, b, 3] != 1) {
                            // устанавливаем данный пиксель на результирующее изображение
                            resaultBitmap.SetPixel(a, b, Color.FromArgb(tmp_layer_data[a, b, 0], tmp_layer_data[a, b, 1], tmp_layer_data[a, b, 2]));
                        } else {
                            if (ax == 0) // нулевой слой - необходимо закрасить белым остутствующие пиксели
                            {
                                // закрашиваем белым цветом
                                resaultBitmap.SetPixel(a, b, Color.FromArgb(255, 255, 255));
                            }
                        }
                    }
                }

            }

            // поворачиваем изображение для корректного отображения
            resaultBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);

            // возвращаем результат
            return resaultBitmap;
        }

        // получение изображения для главного слоя
        public void SetImageToMainLayer(Bitmap layer) {
            // поворачиваем изображение (чтобы оно корректно отображалось в области редактирования)
            layer.RotateFlip(RotateFlipType.Rotate180FlipX);

            // проходим двумя циклами по всем пикселям изображения, подгруженного в класс Bitmap
            // получая цвет пикселя, устанавливаем его в текущий слой с помощью функции Drawing
            // данный алгоритм является медленным, но простым
            // оптимальным решением здесь было бы написание собственного загрузчика файлов изображений,
            // что дало бы возможность без "посредников" получать массив значений пикселей изображений,
            // но данная задача является намного более сложной и выходит за рамки обучающей программы

            for (int ax = 0; ax < layer.Width; ax++) {
                for (int bx = 0; bx < layer.Height; bx++) {
                    // получения цвета пикселя изображения
                    SetColor(layer.GetPixel(ax, bx));
                    // отрисовка данного пикселя в слое
                    Drawing(ax, bx);
                }
            }
        }
    }
}
