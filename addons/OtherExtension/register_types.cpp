#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/godot.hpp>

#include "copy_getnode_plugin.cpp"

using namespace godot;

void initialize_copy_getnode_plugin(ModuleInitializationLevel p_level) {
    if (p_level != MODULE_INITIALIZATION_LEVEL_EDITOR) {
        return;
    }
    ClassDB::register_class<CopyGetNodePlugin>();
}

void uninitialize_copy_getnode_plugin(ModuleInitializationLevel p_level) {
    if (p_level != MODULE_INITIALIZATION_LEVEL_EDITOR) {
        return;
    }
}

extern "C" {
    // Вызывается при загрузке плагина
    GDExtensionBool GDE_EXPORT copy_getnode_plugin_init(GDExtensionInterfaceGetProcAddress p_interface, const GDExtensionClassLibraryPtr p_library, GDExtensionInitialization* r_initialization) {
        godot::GDExtensionBinding::InitObject init_obj(p_interface, p_library, r_initialization);
        init_obj.register_initializer(initialize_copy_getnode_plugin);
        init_obj.register_terminator(uninitialize_copy_getnode_plugin);
        init_obj.set_minimum_library_initialization_level(MODULE_INITIALIZATION_LEVEL_EDITOR);
        return init_obj.init();
    }
}
