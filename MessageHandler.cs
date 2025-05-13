namespace Markwardt;

[DefaultImplementation<DefaultHandler>]
public interface IMessageHandler
{
    ValueTask Handle(Message message);
}