#include <godot_cpp/classes/editor_plugin.hpp>
#include <godot_cpp/classes/editor_interface.hpp>
#include <godot_cpp/classes/popup_menu.hpp>
#include <godot_cpp/classes/display_server.hpp>
#include <godot_cpp/classes/editor_selection.hpp>
#include <godot_cpp/classes/scene_tree_dock.hpp>
#include <godot_cpp/classes/node.hpp>
#include <godot_cpp/core/class_db.hpp>

using namespace godot;

class CopyGetNodePlugin : public EditorPlugin {
    GDCLASS(CopyGetNodePlugin, EditorPlugin);

private:
    PopupMenu* popup_menu;
    static constexpr int COPY_GET_NODE_OPTION = 1000;

protected:
    static void _bind_methods() {}

public:
    void _enter_tree() override {
        popup_menu = memnew(PopupMenu);
        popup_menu->connect("id_pressed", callable_mp(this, &CopyGetNodePlugin::on_menu_item_pressed));

        get_editor_interface()->get_base_control()->add_child(popup_menu);
        get_editor_interface()->get_scene_tree_dock()->connect("gui_input", callable_mp(this, &CopyGetNodePlugin::on_gui_input));
    }

    void _exit_tree() override {
        get_editor_interface()->get_scene_tree_dock()->disconnect("gui_input", callable_mp(this, &CopyGetNodePlugin::on_gui_input));
        popup_menu->queue_free();
    }

    void on_gui_input(const Ref<InputEvent>& event) {
        Ref<InputEventMouseButton> mouse_event = event;
        if (mouse_event.is_null() || mouse_event->get_button_index() != MouseButton::RIGHT || !mouse_event->is_pressed()) {
            return;
        }

        EditorSelection* selection = get_editor_interface()->get_selection();
        Array selected_nodes = selection->get_selected_nodes();
        if (selected_nodes.is_empty()) {
            return;
        }

        Node* selected_node = Object::cast_to<Node>(selected_nodes[0]);
        if (!selected_node) {
            return;
        }

        String class_name = selected_node->get_class();
        String node_path = selected_node->get_path();
        String copy_text = vformat("GetNode<%s>(\"%s\")", class_name, node_path);

        popup_menu->clear();
        popup_menu->add_item("Скопировать " + copy_text, COPY_GET_NODE_OPTION);
        popup_menu->set_position(mouse_event->get_global_position());
        popup_menu->popup();
    }

    void on_menu_item_pressed(int id) {
        if (id == COPY_GET_NODE_OPTION) {
            EditorSelection* selection = get_editor_interface()->get_selection();
            Array selected_nodes = selection->get_selected_nodes();
            if (selected_nodes.is_empty()) {
                return;
            }

            Node* selected_node = Object::cast_to<Node>(selected_nodes[0]);
            if (!selected_node) {
                return;
            }

            String copy_text = vformat("GetNode<%s>(\"%s\")", selected_node->get_class(), selected_node->get_path());
            DisplayServer::get_singleton()->clipboard_set(copy_text);
        }
    }
};
