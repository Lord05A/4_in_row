using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

    public Vector2 gridPosition = Vector2.zero;
    public int x;
    public int y;

    private Game game = null;
        
    // Use this for initialization
    void Start()
    {
        x = (int)gridPosition.x / 10;
        y = (int)gridPosition.y / 10;
    }

    public void set_game(Game game)
    {
        this.game = game;
    }
    
    public void change_color(Color color)
    {
        transform.GetComponent<Renderer>().material.color = color;
    }

    public Color get_color()
    {
        return transform.GetComponent<Renderer>().material.color;
    }

    public void OnMouseDown()
    {
        if (!game.game_over && game.can_turn_in_this(x, y))
        {
            game.player_turn(x, y);
            //Debug.Log("AI_think");
            game.AI_turn();
        }        
    }
}
