using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{

    struct Triple
    {
        int length;
        Color first;
        Color second;
    }

    public int GAME_TARGET_VALUE = 4;
    private int ai_num = 2; //ещё не везде исправленно
    private int pl_num = 1; //ещё не везде исправленно
    //public int DEPTH = 4;

    Color player_color = Color.blue;
    Color ai_color = Color.red;

    List<List<int>> field = new List<List<int>>();

    List<List<Tile>> tiles = new List<List<Tile>>();

    public GameObject TilePrefab;

    public int width = 7;
    public int height = 6;

    public bool game_over = false;
    public bool f = false;
    List<int> levels = new List<int>();

    int all_tiles;

    List<int> scores = new List<int>();

    // Use this for initialization
    void Start()
    {
        generate_field();
        generate_hidden_field();
        if (f)
            AI_turn();
    }

    // 1 - игрок
    // 2 - комп
    // 0 - пусто
    // 5 - граница
    void generate_hidden_field()
    {
        levels.Add(height + 1);
        for (int i = 1; i < width + 1; ++i)
        {
            levels.Add(1);
        }
        levels.Add(height + 1);

        for (int i = 0; i < width + 2; ++i)
        {
            field.Add(new List<int>());
            for (int j = 0; j < height + 2; ++j)
            {
                field[i].Add(0);
            }
        }

        for (int i = 0; i < width + 2; ++i)
        {
            field[i][0] = 5;
            field[i][height + 1] = 5;

            tiles[i][0].change_color(Color.black);
            tiles[i][height + 1].change_color(Color.black);
        }

        for (int j = 0; j < height + 2; ++j)
        {
            field[0][j] = 5;
            field[width + 1][j] = 5;

            tiles[0][j].change_color(Color.black);
            tiles[width + 1][j].change_color(Color.black);
        }

        all_tiles = width * height;

        scores.Add(-100500);
        for (int i = 1; i < width + 1; ++i)
        {
            scores.Add(0);
        }
    }

    //проверки должны быть в обе стороны в любом случае.
    //второй инт говорит,что на конце наша пустая штука или нет KeyValuePair<int, int> 
    public int go_len(int x, int y, int step_x, int step_y, int player, ref List<List<int>> f) //player сделать цветом
    {
        x += step_x;
        y += step_y;

        if (x > width || y > height)
            return 0;

        if (f[x][y] == player) //если это цвет свой, то идём дальше, иначе 0
            return 1 + go_len(x, y, step_x, step_y, player, ref f);
        return 0;
    }


    void generate_field()
    {
        tiles = new List<List<Tile>>();
        List<Tile> row;
        for (int i = 0; i < width + 2; ++i)
        {
            row = new List<Tile>();
            for (int j = 0; j < height + 2; ++j)
            {
                Tile tile = ((GameObject)Instantiate(TilePrefab, new Vector3(i * 10, j * 10, 0),
                                                     Quaternion.Euler(new Vector2()))).GetComponent<Tile>();
                tile.gridPosition = new Vector2(i * 10, j * 10);
                row.Add(tile);
                tile.set_game(this);
            }
            tiles.Add(row);
        }
    }

    public bool can_turn_in_this(int x, int y)
    {
        if (levels[x] == y &&
            tiles[x][y].get_color() == Color.white)
            return true;
        return false;
    }

    public int max_length(int x, int y, int player, ref List<List<int>> f)
    {
        return Mathf.Max(
            1 + go_len(x, y, -1, 0, player, ref f) + go_len(x, y, 1, 0, player, ref f),
            1 + go_len(x, y, 0, 1, player, ref f) + go_len(x, y, 0, -1, player, ref f),
            1 + go_len(x, y, 1, 1, player, ref f) + go_len(x, y, -1, -1, player, ref f),
            1 + go_len(x, y, -1, 1, player, ref f) + go_len(x, y, 1, -1, player, ref f));
    }

    Color color_in_the_end(int x, int y, int step_x, int step_y, Color c)
    {
        x += step_x;
        y += step_y;

        if (tiles[x][y].get_color() == c)
            return color_in_the_end(x, y, step_x, step_y, c);

        return tiles[x][y].get_color();
    }

    public void AI_turn()
    {
        if (game_over)
            return;

        List<int> priority = new List<int>();
        priority.Add(-100500);        

        List<List<List<int>>> Fields = new List<List<List<int>>>();
        for (int i = 0; i < width + 1; ++i)
        {
            Fields.Add(new List<List<int>>(field));
        }


            for (int i = 1; i < width + 1; ++i)
            {
                if (levels[i] <= height)
                {
                    List<List<int>> f = Fields[i];
                    do_step(i, ai_num, ref f);
                    //priority.Add(score_ai(i, levels[i] - 1, DEPTH));
                    StartCoroutine(turn1(i, levels[i] - 1, f));
                    priority.Add(scores[i] + width / 2 - Mathf.Abs(width / 2 - i + 1));
                    undo_step(i, ref f);
                }
                else priority.Add(-100500); //достигли здесь потолка
            }

        debug_log_list(priority);
                
        int max = -100400;
        int ind = 1;
        for (int i = 1; i < width + 1; ++i)
        {
            if (priority[i] > max)
            {
                max = priority[i];
                ind = i;
            }
        }
       
        StartCoroutine(do_turn(ind, levels[ind], 2, ai_color));       
    }

    public List<int> do_grades(int player, ref List<List<int>> fi, bool f = false)
    {
        int a = 0;
        if (f)
            ++a;

        List<int> grades = new List<int>();
        grades.Add(-1);
        for (int i = 1; i < width + 1; ++i)
        {
            if (i >= width + 1 || levels[i] >= height + a)
                grades.Add(-100600);
            else
                grades.Add(max_length(i, levels[i] + a, player, ref fi));
        }
        return grades;
    }


    public void player_turn(int x, int y)
    {
        StartCoroutine(do_turn(x, y, 1, player_color));        
    }

    public IEnumerator do_turn(int x, int y, int p, Color c)
    {        
        do_step(x, p, ref field);
        tiles[x][y].change_color(c);
        --all_tiles;
        last_turn(x, y, p);        
        yield return null;
    }

    private void last_turn(int x, int y, int p)
    {
        if (all_tiles == 0 || max_length(x, y, p, ref field) >= GAME_TARGET_VALUE)
        {
            game_over = true;            
        }
    }

    private void debug_log_field()
    {
        for (int i = 0; i < field.Count; ++i)
        {
            string str = "";
            for (int j = 0; j < field[i].Count; ++j)
            {
                str += field[i][j];
            }
            Debug.Log(str);
        }
    }

    private void debug_log_list(List<int> a)
    {
        string str = "";
        for (int i = 0; i < a.Count; ++i)
        {
            str += a[i] + ";";
        }
        Debug.Log(str);
    }

    private void debug_log_list(List<List<int>> a)
    {
        for (int i = 0; i < a.Count; ++i)
        {
            string str = "";
            for (int j = 0; j < a[i].Count; ++j)
            {
                str += a[i][j];
            }
        }
    }

    private void do_step(int x, int p, ref List<List<int>> f)
    {
        f[x][levels[x]] = p;
        levels[x]++;
    }
    private void undo_step(int x, ref List<List<int>> f)
    {
        levels[x]--;
        f[x][levels[x]] = 0;
    }

    int turn6(int x, int y, ref List<List<int>> f)
    {
        int len = max_length(x, y, pl_num, ref f);
        if (len >= GAME_TARGET_VALUE)
            return -200;
        return 5 + len;
    }

    int turn5(int x, int y, ref List<List<int>> f)
    {
        int len = max_length(x, y, ai_num, ref f);
        if (len >= GAME_TARGET_VALUE)
            return 350;
        
        int min = 1000;
        for (int i = 1; i < width + 1; ++i)
        {
            int x1 = i;
            int y1 = levels[i];
            
            int tmp = min / 2;
            if (y1 < height + 1)
            {
                do_step(x1, pl_num, ref f);
                tmp = turn6(x1, y1, ref f);
                undo_step(x1, ref f);
            }
            
            if (tmp < min) min = tmp;
        }
       
        return min;
    }

    int turn4(int x, int y, ref List<List<int>> f)
    {
        int len = max_length(x, y, pl_num, ref f);
        if (len >= GAME_TARGET_VALUE)
            return -400;

        int max = -1000;
       
        for (int i = 1; i < width + 1; ++i)
        {
            int x1 = i;
            int y1 = levels[i];
            int tmp = max / 2;
           
            if (y1 < height + 1)
            {
                do_step(x1, ai_num, ref f);
                tmp = turn5(x1, y1, ref f);
                undo_step(x1, ref f);
            }

            if (tmp > max) max = tmp;
            
        }
        return max;        
    }

    int turn3(int x, int y, ref List<List<int>> f)
    {
        int len = max_length(x, y, ai_num, ref f);
        if (len >= GAME_TARGET_VALUE)
            return 450;

        int min = 1000;
        for (int i = 1; i < width + 1; ++i)
        {
            int x1 = i;
            int y1 = levels[i];
           
            int tmp = min / 2;
            if (y1 < height + 1)
            {
                do_step(x1, pl_num, ref f);
                tmp = turn4(x1, y1, ref f);
                undo_step(x1, ref f);
            }

            if (tmp < min)
                min = tmp;
        }
       
        return min;
    }

    int turn2(int x, int y, ref List<List<int>> f)
    {
        int len = max_length(x, y, pl_num, ref f);
        if (len >= GAME_TARGET_VALUE)
            return -500;

        int max = -1000;
       
        for (int i = 1; i < width + 1; ++i)
        {
            int x1 = i;
            int y1 = levels[i];
            int tmp = max / 2;
            
            if (y1 < height + 1)
            {
                do_step(x1, ai_num, ref f);
                tmp = turn3(x1, y1, ref f);
                undo_step(x1, ref f);
            }

            if (tmp > max) max = tmp;           
        }
        return max;        
    }



    IEnumerator turn1(int x, int y, List<List<int>> f)
    {
        int len = max_length(x, y, ai_num, ref f);
        if (len >= GAME_TARGET_VALUE)
        {
            scores[x] = 550;
            yield return 550;
        }
       
        int min = 1000;
        for (int i = 1; i < width + 1; ++i)
        {
            int x1 = i;
            int y1 = levels[i];
            
            int tmp = min / 2;
            if (y1 < height + 1)
            {
                do_step(x1, pl_num, ref f);
                tmp = turn2(x1, y1, ref f);
                undo_step(x1, ref f);
            }
           
            if (tmp < min) min = tmp;
        }
        
        scores[x] = min;
        yield return min;
    }

    int turn0(int x, int y, ref List<List<int>> f)
    {
        int len = max_length(x, y, pl_num, ref f);
        if (len >= GAME_TARGET_VALUE)
            return -600;

        int max = -1000;
       
        for (int i = 1; i < width + 1; ++i)
        {
            int x1 = i;
            int y1 = levels[i];
            int tmp = max / 2;
            
            if (y1 < height + 1)
            {
                do_step(x1, ai_num, ref f);
                
                undo_step(x1, ref f);
            }

            if (tmp > max) max = tmp;
            
        }
        return max;
        
    }

    int turn00(int x, int y, ref List<List<int>> f)
    {
        int len = max_length(x, y, ai_num, ref f);
        if (len >= GAME_TARGET_VALUE)
            return 650;

       
        int min = 1000;
        for (int i = 1; i < width + 1; ++i)
        {
            int x1 = i;
            int y1 = levels[i];
            
            int tmp = min / 2;
            if (y1 < height + 1)
            {
                do_step(x1, pl_num, ref f);
                tmp = turn0(x1, y1, ref f);
                undo_step(x1, ref f);
            }           
            if (tmp < min) min = tmp;
        }
       
        return min;
    }
}