## IngoBot Discord Bot Documentation

### Text Commands
Text commands can be used by simply sending a message prefixed by the *command prefix* (```!``` by default), followed by the arguments e.g. ```!sticker IngoDog_hi Hello!```.

#### Image commands
##### !sticker
Syntax: ```!sticker <name> <text>```  
Sends the sticker with name ```name``` and text ```text```.
For an explanation on how to create stickers, refer to [STICKERS.md](https://github.com/IngoHHacks/IngoBot/blob/main/STICKERS.md)

#### Chatbot commands
**IMPORTANT!**  
Chatbot commands are disabled by default.
##### !c !fs !s and !furry
Syntax: ```!c/fs/s/furry <prompt>```  
Generates a response for the given prompt using the respective model.
For more information about the models, refer to [CHATBOT.md](https://github.com/IngoHHacks/IngoBot/blob/main/CHATBOT.md)

### Slash Commands
Slash commands can be executed by typing a slash (```/```), then following the Discord slash command interface. The syntax works the same as text commands.

#### Management Commands
##### /setprefix
Syntax: ```/setprefix <prefix>```  
Sets the text commands prefix to ```prefix```. There are no restrictions on the prefix.

##### /enablecommand
Syntax: ```/enablecommand <command>```  
Enables a text command.

##### /disablecommand
Syntax: ```/disablecommand <command>```  
Disables a text command.

#### Other
##### /help
Syntax: ```/help```  
Sends a link to this documentation document.


### Context Commands
Context commands can be used by right clicking on either a user or a message, selecting *Apps*, then choosing the respective command.

#### User Commands
##### Get Avatar
Sends a 2048x image of the selected user's avatar (profile picture).

#### Message Commands
##### Stickerify
Creates a dialog to send a sticker with the selected message's text.