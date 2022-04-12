## IngoBot Chatbot

### Explanation
The IngoBot Chatbot runs from the [Chatbot API](https://ingoh.net/api/sandbox).  

There are three models:
### GPT-2-MC
GPT-2-MC is the largest and most fine-tuned model. It was trained on the chat history of SimpleFlips' Minecraft Server, and can reasonably replicate server chat, as well as give somewhat coherent responses to prompts.

### GPT-Neo
GPT-Neo is the most generic model. It has not been fine-tuned, so it can write about many different topics and with many different styles, but it is worse at understanding input prompts.

### GPT-2-FURRY
GPT-2-FURRY is the weakest model. It was made as an April Fools joke. It can write stories featuring anthropomorphic animals, but do not expect too much of it.

### Commands
```!c``` uses the GPT-2-MC model and returns the second line.  
```!fs``` uses the GPT-2-MC model and returns the first line.  
```!s``` uses the GPT-Neo model and returns the entire content.  
```!furry``` uses the GPT-2-FURRY model and returns the first line.