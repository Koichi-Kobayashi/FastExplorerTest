using System.Reflection;
using Codeer.Friendly;
using Codeer.Friendly.Windows;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace FastExplorerDriver
{
    /// <summary>
    /// IntPtr用のMessagePackフォーマッター
    /// </summary>
    public class IntPtrFormatter : IMessagePackFormatter<IntPtr>
    {
        public void Serialize(ref MessagePackWriter writer, IntPtr value, MessagePackSerializerOptions options)
            => writer.Write(value.ToInt64());

        public IntPtr Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            => new IntPtr(reader.ReadInt64());
    }

    /// <summary>
    /// UIntPtr用のMessagePackフォーマッター
    /// </summary>
    public class UIntPtrFormatter : IMessagePackFormatter<UIntPtr>
    {
        public void Serialize(ref MessagePackWriter writer, UIntPtr value, MessagePackSerializerOptions options)
            => writer.Write(value.ToUInt64());

        public UIntPtr Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            => new UIntPtr(reader.ReadUInt64());
    }

    /// <summary>
    /// Friendly用のカスタムシリアライザー（MessagePackを使用）
    /// </summary>
    public class CustomSerializer : ICustomSerializer
    {
        private readonly MessagePackSerializerOptions _options;

        public CustomSerializer()
        {
            // より包括的なリゾルバーを使用
            _options = MessagePackSerializerOptions.Standard
                .WithResolver(
                    CompositeResolver.Create(
                        new IMessagePackFormatter[] 
                        { 
                            new IntPtrFormatter(),
                            new UIntPtrFormatter()
                        },
                        new IFormatterResolver[] 
                        { 
                            TypelessContractlessStandardResolver.Instance,
                            StandardResolver.Instance
                        }
                    )
                )
                .WithCompression(MessagePackCompression.None)
                .WithSecurity(MessagePackSecurity.UntrustedData);
        }

        public object? Deserialize(byte[] bin)
        {
            try
            {
                return MessagePackSerializer.Typeless.Deserialize(bin, _options);
            }
            catch (Exception ex)
            {
                // デシリアライゼーションエラーの詳細をログに出力
                System.Diagnostics.Debug.WriteLine($"Deserialize error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public Assembly[] GetRequiredAssemblies() 
            => new[] 
            { 
                GetType().Assembly, 
                typeof(MessagePackSerializer).Assembly,
                typeof(ICustomSerializer).Assembly
            };

        public byte[] Serialize(object obj)
        {
            try
            {
                return MessagePackSerializer.Typeless.Serialize(obj, _options);
            }
            catch (Exception ex)
            {
                // シリアライゼーションエラーの詳細をログに出力
                System.Diagnostics.Debug.WriteLine($"Serialize error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Type: {obj?.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}

