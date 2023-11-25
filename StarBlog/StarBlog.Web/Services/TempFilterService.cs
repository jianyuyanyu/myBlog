using System.Text.Json;
using CodeLab.Share.Contrib.StopWords;

namespace StarBlog.Web.Services;

public class TempFilterService {
    private readonly StopWordsToolkit _toolkit;

    public StopWordsToolkit Toolkit => _toolkit;

    public TempFilterService() {
        var words = JsonSerializer.Deserialize<IEnumerable<Word>>(File.ReadAllText("words.json"));
        _toolkit = new StopWordsToolkit(words!.Select(a => a.Value));
    }

    public bool CheckBadWord(string word) {
        return _toolkit.CheckBadWord(word);
    }
}
/*
public class Word
{
	public int Id { get; set; }

	public string Value { get; set; }

	public string? Tag { get; set; }
}
internal class WordNode
{
	private readonly Dictionary<int, WordNode> node;

	private byte _isEnd;

	public WordNode()
	{
		node = new Dictionary<int, WordNode>();
	}

	public void Add(string word)
	{
		if (word.Length > 0)
		{
			int key = word[0];
			if (!node.ContainsKey(key))
			{
				node.Add(key, new WordNode());
			}
			WordNode wordNode = node[key];
			if (word.Length > 1)
			{
				wordNode.Add(word.Substring(1));
			}
			else
			{
				wordNode._isEnd = 1;
			}
		}
	}

	public int CheckAndGetEndIndex(string sourceDBCText, int cursor, Func<char, bool>? checkSpecialSym)
	{
		for (int i = cursor; i < sourceDBCText.Length; i++)
		{
			if (checkSpecialSym == null)
			{
				break;
			}
			if (!checkSpecialSym(sourceDBCText[i]))
			{
				break;
			}
			cursor++;
		}
		if (cursor >= sourceDBCText.Length)
		{
			return -1;
		}
		int key = sourceDBCText[cursor];
		if (node.ContainsKey(key))
		{
			if (node[key]._isEnd == 1)
			{
				return cursor;
			}
			return node[key].CheckAndGetEndIndex(sourceDBCText, cursor + 1, checkSpecialSym);
		}
		return -1;
	}
}
public class StopWordsToolkit
{
	private struct RNode
	{
		public int start;

		public int len;

		public int type;
	}

	private readonly WordNode? _wordNodes;

	public StopWordsToolkit(IEnumerable<string> words)
	{
		_wordNodes = new WordNode();
		foreach (string word in words)
		{
			if (!string.IsNullOrWhiteSpace(ToDBC(word)))
			{
				_wordNodes.Add(word);
			}
		}
	}

	public bool CheckBadWord(string sourceText, int filterNum = 0)
	{
		if (string.IsNullOrWhiteSpace(sourceText))
		{
			return false;
		}

		string text = ToDBC(sourceText);
		for (int i = 0; i < text.Length; i++)
		{
			if (filterNum > 0 && IsNum(text[i]) && CheckNumberSeq(text, i, filterNum) > 0)
			{
				return true;
			}

			if (Check(text, i) > 0)
			{
				return true;
			}
		}

		return false;
	}

	public string FilterWithChar(string sourceText, char replaceChar, int filterNum = 0, string numReplace = null)
	{
		if (sourceText != string.Empty)
		{
			string text = ToDBC(sourceText);
			char[] array = sourceText.ToCharArray();
			List<RNode> list = new List<RNode>();
			for (int i = 0; i < text.Length; i++)
			{
				int num = 0;
				if (filterNum > 0 && IsNum(text[i]))
				{
					num = CheckNumberSeq(text, i, filterNum);
					if (num > 0)
					{
						num++;
						if (numReplace == null)
						{
							for (int j = 0; j < num; j++)
							{
								array[j + i] = replaceChar;
							}
						}
						else
						{
							list.Add(new RNode
							{
								start = i,
								len = num,
								type = 1
							});
						}

						i = i + num - 1;
						continue;
					}
				}

				num = Check(text, i);
				if (num > 0)
				{
					for (int k = 0; k < num; k++)
					{
						array[k + i] = replaceChar;
					}

					i = i + num - 1;
				}
			}

			if (list.Count > 0)
			{
				return ReplaceString(array, list, null, numReplace);
			}

			return new string(array);
		}

		return string.Empty;
	}

	public string FilterWithStr(string sourceText, string replaceStr, int filterNum = 0, string numReplace = null)
	{
		if (sourceText != string.Empty)
		{
			string text = ToDBC(sourceText);
			List<RNode> list = new List<RNode>();
			if (filterNum > 0 && numReplace == null)
			{
				numReplace = replaceStr;
			}

			int num = 0;
			for (int i = 0; i < text.Length; i++)
			{
				if (filterNum > 0 && IsNum(text[i]))
				{
					num = CheckNumberSeq(text, i, filterNum);
					if (num > 0)
					{
						num++;
						int start = i;
						list.Add(new RNode
						{
							start = start,
							len = num,
							type = 1
						});
						i = i + num - 1;
						continue;
					}
				}

				num = Check(text, i);
				if (num > 0)
				{
					list.Add(new RNode
					{
						start = i,
						len = num,
						type = 0
					});
					i = i + num - 1;
				}
			}

			return ReplaceString(sourceText.ToCharArray(), list, replaceStr, numReplace);
		}

		return string.Empty;
	}

	private static string ReplaceString(char[] charArry, List<RNode> nodes, string replaceStr, string numReplace)
	{
		if (string.IsNullOrWhiteSpace(numReplace))
		{
			numReplace = replaceStr;
		}

		if (string.IsNullOrWhiteSpace(replaceStr))
		{
			replaceStr = numReplace;
		}

		List<char> list = new List<char>(charArry);
		int num = 0;
		int i = 0;
		for (int count = nodes.Count; i < count; i++)
		{
			int num2 = nodes[i].start + num;
			int len = nodes[i].len;
			int num3 = num2 + len - 1;
			string text = ((nodes[i].type == 0) ? replaceStr : numReplace);
			if (text.Length < len)
			{
				list.RemoveRange(num2, len - text.Length);
			}

			int j = 0;
			for (int length = text.Length; j < length; j++)
			{
				char c = text[j];
				int num4 = num2 + j;
				if (num4 <= num3)
				{
					list[num4] = c;
				}
				else
				{
					list.Insert(num4, c);
				}
			}

			num += text.Length - len;
		}

		return new string(list.ToArray());
	}

	private int Check(string sourceText, int cursor)
	{
		int num = _wordNodes.CheckAndGetEndIndex(sourceText, cursor, CheckSpecialSym);
		if (num < cursor)
		{
			return 0;
		}

		return num - cursor + 1;
	}

	private static int CheckNumberSeq(string sourceText, int cursor, int filterNum)
	{
		int num = 0;
		if (cursor + 1 >= sourceText.Length)
		{
			return 0;
		}

		for (int i = cursor + 1; i < sourceText.Length && IsNum(sourceText[i]); i++)
		{
			num++;
		}

		if (num + 1 >= filterNum)
		{
			return num;
		}

		return 0;
	}

	private static bool CheckSpecialSym(char character)
	{
		if (!IsChinese(character) && !IsNum(character))
		{
			return !IsAlphabet(character);
		}

		return false;
	}

	private static bool IsChinese(char character)
	{
		if (character >= '一')
		{
			return character <= '龥';
		}

		return false;
	}

	private static bool IsNum(char character)
	{
		if (character >= '0')
		{
			return character <= '9';
		}

		return false;
	}

	private static bool IsAlphabet(char character)
	{
		if (character < 'a' || character > 'z')
		{
			if (character >= 'A')
			{
				return character <= 'Z';
			}

			return false;
		}

		return true;
	}

	private string ToDBC(string input)
	{
		char[] array = input.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '\u3000')
			{
				array[i] = ' ';
			}
			else if (array[i] > '\uff00' && array[i] < '｟')
			{
				array[i] = (char)(array[i] - 65248);
			}
		}

		return new string(array).ToLower();
	}
}
*/