import sys
import os
import tkinter as tk
from tkinter import ttk
import tkinter.messagebox as messagebox
from PIL import Image, ImageTk
from ctypes import *
import live_view_control
import camera_control
import datetime
import threading
import io
import platform


def message_load_err():
    os_name = platform.system()
    if os_name == "Windows":
        messagebox.showinfo("ERR", "Failed to load XAPI.dll")
    if os_name == "Darwin":
        messagebox.showinfo("ERR", "Failed to load XAPI.bundle")
    if os_name == "Linux":
        messagebox.showinfo("ERR", "Failed to load XAPI.so")


def message_argument_err():
    messagebox.showinfo("ERR", "argument err")


def message_sdk_err():
    global is_connect_run
    global error_window
    is_connect_run = False
    
    err_info = _live_view_control.get_error_code()
    error_window_label.config(text=err_info)
    center_window(error_window, 350, 150)
    error_window.deiconify()


def disable_button():
    button.config(state="disabled")
    combo.config(state="disabled")


def enable_button():
    button.config(state="normal")
    combo.config(state="readonly")
    on_check()


def on_check():
    _live_view_control.on_check(var.get())


def get_exposure_bias(event):
    selected_bias = combo.get()
    thread_update_exposure_bias = threading.Thread(target=get_exposure_bias_thread, args=(selected_bias,))
    thread_update_exposure_bias.daemon = True
    thread_update_exposure_bias.start()


def get_exposure_bias_thread(selected_exposure_bias):
    global exposure_bias_list
    global exposure_bias
    global is_thread_run
    global is_live_view_run
    global thread
    is_live_view_run = False
    is_thread_run = False
    thread.join()

    combo['values'] = exposure_bias_list
    get_exposure_bias = c_long(0)
    result = _live_view_control.set_exposure_bias(selected_exposure_bias, get_exposure_bias)
    if result != camera_control.XSDK_COMPLETE:
        message_sdk_err()
    else:
        combo.set(live_view_control.exposure_bias_dict[get_exposure_bias.value])
    is_thread_run = True
    is_live_view_run = True
    thread = threading.Thread(target=live_view)
    thread.daemon = True
    thread.start()
    
    
def release():
    thread_release = threading.Thread(target=release_thread)
    thread_release.daemon = True
    thread_release.start()


def release_thread():
    global is_thread_run
    global is_live_view_run
    global thread
    is_live_view_run = False
    is_thread_run = False
    thread.join()
    
    result = _live_view_control.release()
    if result != camera_control.XSDK_COMPLETE:
        message_sdk_err()
    is_thread_run = True
    is_live_view_run = True
    thread = threading.Thread(target=live_view)
    thread.daemon = True
    thread.start()


def live_view():
    global is_thread_run
    global is_connect_run
    global is_live_view_run
    global is_live_view_start
    global exposure_bias_list
    global exposure_bias
    global combo
    global button
    global waiting_img
    global root
    
    while is_thread_run:
        while is_connect_run:
            root.after(0, disable_button)
            result = _live_view_control.initialize()
            if result == live_view_control.SDK_LOAD_ERROR:
                message_load_err()
                is_thread_run = False
                is_connect_run = False
                root.quit()
                root.destroy()
                sys.exit(0)
            
            result = _live_view_control.camera_conect(sys.argv)
            if result == live_view_control.ARGV_ERROR:
                message_argument_err()
                _live_view_control.live_view_stop()
                root.quit()
                root.destroy()
                sys.exit(0)
            if result == live_view_control.NOT_FOUND_CAMERA:
                _live_view_control.live_view_stop()
                continue
            elif result == camera_control.XSDK_ERROR:
                _live_view_control.live_view_stop()
                continue
            
            exposure_bias_list = []
            exposure_bias = c_long(0)
            result = _live_view_control.view_setting(exposure_bias_list, exposure_bias)
            if result != camera_control.XSDK_COMPLETE:
                _live_view_control.live_view_stop()
                continue
            
            combo['values'] = exposure_bias_list
            combo.set(live_view_control.exposure_bias_dict[exposure_bias.value])
            
            result = _live_view_control.live_view_start()
            if result != camera_control.XSDK_COMPLETE:
                is_live_view_start = False
            else:
                is_live_view_start = True
            
            is_connect_run = False
            is_live_view_run = True
            root.after(0, enable_button)
        
        while is_live_view_run:
            data = _live_view_control.read_image()
            if data is not None and is_live_view_start:
                with Image.open(io.BytesIO(data)) as img:
                    img = img.resize((550, 400))
                    img_tk = ImageTk.PhotoImage(img)
                    root.after(0, update_image, img_tk)
            elif not is_live_view_start:
                img_tk2 = ImageTk.PhotoImage(waiting_img)
                root.after(0, update_image, img_tk2)
            if _live_view_control.is_communicatom_error:
                is_live_view_run = False
                img_tk3 = ImageTk.PhotoImage(waiting_img)
                root.after(0, update_image, img_tk3)
                message_sdk_err()
                _live_view_control.live_view_stop()
                root.after(0, disable_button)


def update_image(img_tk):
    image_label.config(image=img_tk)
    image_label.image = img_tk


def view_close():
    thread_view_close = threading.Thread(target=view_close_thread)
    thread_view_close.daemon = True
    thread_view_close.start()


def view_close_thread():
    global _live_view_control;
    global is_thread_run
    global is_connect_run
    global is_live_view_run
    global is_live_view_run
    global thread
    global root
    is_thread_run = False
    is_connect_run = False
    is_live_view_run = False
    thread.join()
    
    _live_view_control.live_view_stop()
    del _live_view_control
    _live_view_control = None
    root.quit()
    root.destroy()
    sys.exit(0)


def error_window_close():
    global is_connect_run
    global is_live_view_run
    global error_window
    
    error_window.withdraw()
    if not is_live_view_run:
        is_connect_run = True
        root.after(0, disable_button)


def center_window(window, width, height):
    screen_width = window.winfo_screenwidth()
    screen_height = window.winfo_screenheight()
    position_top = int(screen_height / 2 - height / 2)
    position_right = int(screen_width / 2 - width / 2)
    window.geometry('{0}x{1}+{2}+{3}'.format(width, height, position_right, position_top))


_live_view_control = live_view_control.LiveViewControl()
exposure_bias_list = []
exposure_bias = c_long(0)
is_thread_run = True
is_connect_run = True
is_live_view_run = False
is_live_view_start = False

# Tool view setting
root = tk.Tk()
root.title("Python LiveView")
root.geometry("600x580+50+50")
root.resizable(False, False)
root.protocol("WM_DELETE_WINDOW", view_close)

# Live view image view setting
frame = tk.Frame(root)
frame.pack(padx=5)
center_window(root, 600, 580)
image_label = tk.Label(frame)
image_label.pack(pady=5)
waiting_img = Image.open("view.png")
waiting_img = waiting_img.resize((550, 400))
img_tk = ImageTk.PhotoImage(waiting_img)
image_label.config(image=img_tk)
image_label.image = img_tk

# Button setting
button = tk.Button(frame, text="RELEASE", command=release, width=15)
button.pack(anchor='w', padx=10, pady=10)

# Checkbox setting
var = tk.IntVar()
checkbox = tk.Checkbutton(frame, text="Grab and save images onto the current folder", variable=var, command=on_check)
checkbox.pack(anchor='w', padx=7)

# Label setting
frame2 = tk.LabelFrame(root, text="Camera control")
frame2.pack(padx=10, pady=5, fill="both", expand=True)

# Combobox setting
label2 = tk.Label(frame2, text="Exposure bias :")
label2.pack(side=tk.LEFT, padx=5)
combo = ttk.Combobox(frame2, state="readonly", width=15)
combo['values'] = exposure_bias_list
combo.set(live_view_control.exposure_bias_dict[exposure_bias.value])
combo.bind("<<ComboboxSelected>>", get_exposure_bias)
combo.pack(side=tk.LEFT, padx=5, pady=10)

# error_window setting
error_window = tk.Toplevel(root)
error_window.withdraw()
error_window.title("ERROR")
error_window.geometry("400x150")
error_window_label = tk.Label(error_window, text="")
error_window_label.pack(pady=20)
error_window_button = tk.Button(error_window, text="Close", command=error_window_close)
error_window_button.pack(pady=10)
error_window.protocol("WM_DELETE_WINDOW", error_window_close)

# Start live view in a separate thread
thread = threading.Thread(target=live_view)
thread.daemon = True
thread.start()


root.mainloop()

